import { Injectable, inject } from '@angular/core';
import { CourtHubService } from '../realtime/court-hub.service';
import { ScreenStateService } from '../navigation/screen-state.service';
import { PlayerSessionService } from '../session/player-session.service';
import { RoomStateService } from './room-state.service';
import { MatchStateService } from './match-state.service';
import type { RoomSnapshotDto } from '../../shared/models/room.model';

export type { Screen } from '../navigation/screen-state.service';
export type { CurrentRound } from './match-state.service';

@Injectable({ providedIn: 'root' })
export class GameStateService {
  private readonly screenState = inject(ScreenStateService);
  private readonly playerSession = inject(PlayerSessionService);
  private readonly roomState = inject(RoomStateService);
  private readonly matchState = inject(MatchStateService);

  readonly screen = this.screenState.current;
  readonly room = this.roomState.room;
  readonly yourPlayerId = this.roomState.yourPlayerId;
  readonly errorMessage = this.roomState.errorMessage;
  readonly currentRound = this.matchState.currentRound;
  readonly filedPlayerIds = this.matchState.filedPlayerIds;
  readonly deliberationResults = this.matchState.deliberationResults;
  readonly standings = this.matchState.standings;
  readonly finalStandings = this.matchState.finalStandings;
  readonly winnerPlayerId = this.matchState.winnerPlayerId;
  readonly rematchReadyCount = this.matchState.rematchReadyCount;
  readonly players = this.roomState.players;
  readonly you = this.roomState.you;
  readonly isHost = this.roomState.isHost;

  constructor(private readonly hub: CourtHubService) {
    // A dropped/restored connection gets a brand new SignalR ConnectionId server-side —
    // Context.Items and group membership from before are gone, so re-run the same Rejoin
    // handshake used on page load.
    this.hub.onReconnected(() => void this.attemptRejoin());
  }

  async bootstrap(): Promise<void> {
    await this.hub.connect();
    await this.attemptRejoin();
  }

  /** Re-reads the authoritative room/match snapshot after a missed realtime event. */
  async synchronize(): Promise<void> {
    if (!this.playerSession.read()) return;
    await this.hub.connect();
    await this.attemptRejoin();
  }

  private async attemptRejoin(): Promise<void> {
    const session = this.playerSession.read();
    if (!session) return;

    try {
      const result = await this.hub.rejoin(session.roomCode);
      this.applyIdentity(result.room, result.yourPlayerId);

      if (result.match) {
        this.matchState.restoreRound({
          roundId: result.match.matchId,
          caseNo: result.match.currentRoundIndex + 1,
          caseTotal: result.match.caseCount,
          autos: result.match.autos ?? '',
          hint: result.match.hint ?? '',
          deadlineUtc: result.match.deadlineUtc ?? '',
        }, result.match.filedPlayerIds, result.match.standings);

        if (result.match.roundStatus === 'Open' && result.match.deadlineUtc) {
          if (this.filedPlayerIds().includes(result.yourPlayerId)) this.screenState.showWaiting();
          else this.screenState.showCase();
        } else if (result.match.roundStatus === 'Revealed' && result.match.results) {
          // Replayed from the snapshot rather than waiting for a broadcast that already fired.
          this.matchState.reveal(result.match.results, result.match.standings);
          this.screenState.showVerdict();
        } else {
          // Judging: the round closed but the judge call (or its fallback) hasn't landed
          // yet — land in the waiting room, which shows "EM DELIBERAÇÃO" once it notices
          // everyone's filed, and the DeliberationReady broadcast will carry us forward.
          this.screenState.showWaiting();
        }
      } else {
        this.screenState.showLobby();
      }
    } catch {
      // Stale/expired session — clear it and let the player start fresh from Home.
      this.clearPersistedIdentity();
    }
  }

  async createRoom(hostName: string): Promise<void> {
    await this.hub.connect();
    const result = await this.hub.createRoom(hostName);
    this.applyIdentity(result.room, result.yourPlayerId);
    this.screenState.showLobby();
  }

  async joinRoom(roomCode: string, playerName: string): Promise<void> {
    await this.hub.connect();
    const result = await this.hub.joinRoom(roomCode, playerName);
    this.applyIdentity(result.room, result.yourPlayerId);
    this.screenState.showLobby();
  }

  goHome(): void {
    this.clearPersistedIdentity();
    this.roomState.clear();
    this.screenState.showHome();
  }

  goJoin(): void {
    this.screenState.showJoin();
  }

  updateSettings(caseCount: number, roundDurationSeconds: number): Promise<void> {
    return this.hub.updateSettings(caseCount, roundDurationSeconds);
  }

  startMatch(): Promise<void> {
    return this.hub.startMatch();
  }

  async submitDefense(text: string): Promise<void> {
    this.screenState.showWaiting();
    try {
      await this.hub.submitDefense(text);
    } catch (error) {
      // The screen changes optimistically so the final filing never remains on
      // the testimony form while the server asks Gemini for the verdict.
      // If the submission itself is rejected, let the player correct it.
      this.screenState.showCase();
      throw error;
    }
  }

  nextRound(): Promise<void> {
    return this.hub.nextRound();
  }

  requestRematch(wantsRematch: boolean): Promise<void> {
    return this.hub.requestRematch(wantsRematch);
  }

  clearError(): void {
    this.roomState.clearError();
  }

  showHome(): void { this.screenState.showHome(); }
  showWaiting(): void { this.screenState.showWaiting(); }

  private applyIdentity(room: RoomSnapshotDto, playerId: string): void {
    this.roomState.applyIdentity(room, playerId);
    this.playerSession.save(room.roomCode);
  }

  private clearPersistedIdentity(): void {
    this.playerSession.clear();
  }

}
