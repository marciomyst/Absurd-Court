import { Injectable } from '@angular/core';
import { CourtHubService } from './court-hub.service';
import { ScreenStateService } from '../navigation/screen-state.service';
import { PlayerSessionService } from '../session/player-session.service';
import { RoomStateService } from '../state/room-state.service';
import { MatchStateService } from '../state/match-state.service';
import type {
  CaseStartedEvent,
  DefenseFiledEvent,
  DeliberationReadyEvent,
  MatchEndedEvent,
  PlayerDisconnectedEvent,
  PlayerLeftEvent,
  PlayerJoinedEvent,
  PlayerReconnectedEvent,
  RematchStatusEvent,
  RoomClosedEvent,
  RoomSettingsUpdatedEvent,
} from '../../shared/models/hub-events.model';

@Injectable({ providedIn: 'root' })
export class HubEventBridgeService {
  constructor(
    private readonly hub: CourtHubService,
    private readonly screen: ScreenStateService,
    private readonly session: PlayerSessionService,
    private readonly room: RoomStateService,
    private readonly match: MatchStateService,
  ) {
    this.register();
  }

  private register(): void {
    this.hub.on<PlayerJoinedEvent>('PlayerJoined', (e) => {
      this.room.addPlayer({
        playerId: e.playerId, name: e.playerName, initials: e.initials, isHost: e.isHost, isConnected: true,
      });
    });

    this.hub.on<PlayerReconnectedEvent>('PlayerReconnected', (e) =>
      this.room.updatePlayer(e.playerId, { isConnected: true }),
    );

    this.hub.on<PlayerDisconnectedEvent>('PlayerDisconnected', (e) =>
      this.room.updatePlayer(e.playerId, { isConnected: false }),
    );

    this.hub.on<PlayerLeftEvent>('PlayerLeft', (e) => this.room.removePlayer(e.playerId));

    this.hub.on<RoomSettingsUpdatedEvent>('RoomSettingsUpdated', (e) =>
      this.room.updateSettings(e.caseCount, e.roundDurationSeconds),
    );

    this.hub.on<CaseStartedEvent>('CaseStarted', (e) => {
      this.match.openRound({
        roundId: e.roundId,
        caseNo: e.caseNo,
        caseTotal: e.caseTotal,
        autos: e.autos,
        hint: e.hint,
        deadlineUtc: e.deadlineUtc,
      });
      this.screen.showCase();
    });

    this.hub.on<DefenseFiledEvent>('DefenseFiled', (e) => this.match.fileDefense(e.playerId));

    this.hub.on<DeliberationReadyEvent>('DeliberationReady', (e) => {
      this.match.reveal(e.results, e.standings);
      this.screen.showVerdict();
    });

    this.hub.on<MatchEndedEvent>('MatchEnded', (e) => {
      this.match.finish(e.finalStandings, e.winnerPlayerId);
      this.screen.showEnded();
    });

    this.hub.on<RematchStatusEvent>('RematchStatus', (e) => this.match.setRematchReadyCount(e.readyCount));

    this.hub.on<RoomClosedEvent>('RoomClosed', (e) => {
      this.session.clear();
      this.room.setError(e.reason);
      this.room.clear();
      this.screen.showHome();
    });
  }
}
