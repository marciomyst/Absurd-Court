import { Injectable, signal } from '@angular/core';
import type { RoundResultDto } from '../../shared/models/match.model';

export interface CurrentRound {
  roundId: string;
  caseNo: number;
  caseTotal: number;
  autos: string;
  hint: string;
  deadlineUtc: string;
}

@Injectable({ providedIn: 'root' })
export class MatchStateService {
  readonly currentRound = signal<CurrentRound | null>(null);
  readonly filedPlayerIds = signal<string[]>([]);
  readonly deliberationResults = signal<RoundResultDto[]>([]);
  readonly standings = signal<Record<string, number>>({});
  readonly finalStandings = signal<Record<string, number>>({});
  readonly winnerPlayerId = signal<string | null>(null);
  readonly rematchReadyCount = signal(0);

  restoreRound(round: CurrentRound, filedPlayerIds: string[], standings: Record<string, number>): void {
    this.currentRound.set(round);
    this.filedPlayerIds.set(filedPlayerIds);
    this.standings.set(standings);
  }

  openRound(round: CurrentRound): void {
    this.currentRound.set(round);
    this.filedPlayerIds.set([]);
    this.deliberationResults.set([]);
  }

  fileDefense(playerId: string): void {
    if (!this.filedPlayerIds().includes(playerId))
      this.filedPlayerIds.set([...this.filedPlayerIds(), playerId]);
  }

  reveal(results: RoundResultDto[], standings: Record<string, number>): void {
    this.deliberationResults.set(results);
    this.standings.set(standings);
  }

  finish(finalStandings: Record<string, number>, winnerPlayerId: string): void {
    this.finalStandings.set(finalStandings);
    this.winnerPlayerId.set(winnerPlayerId);
    this.rematchReadyCount.set(0);
  }

  setRematchReadyCount(count: number): void { this.rematchReadyCount.set(count); }
}
