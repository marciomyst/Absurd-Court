import type { RoundResultDto, StandingsByPlayer } from './match.model';

export interface PlayerJoinedEvent {
  playerId: string;
  playerName: string;
  initials: string;
  isHost: boolean;
}

export interface PlayerReconnectedEvent {
  playerId: string;
}

export interface PlayerDisconnectedEvent {
  playerId: string;
}

export interface RoomSettingsUpdatedEvent {
  caseCount: number;
  roundDurationSeconds: number;
}

export interface MatchStartedEvent {
  matchId: string;
  caseCount: number;
}

export interface CaseStartedEvent {
  roundId: string;
  caseNo: number;
  caseTotal: number;
  autos: string;
  hint: string;
  deadlineUtc: string;
}

export interface DefenseFiledEvent {
  playerId: string;
  filedCount: number;
  totalPlayers: number;
}

export interface DeliberationReadyEvent {
  results: RoundResultDto[];
  standings: StandingsByPlayer;
}

export interface MatchEndedEvent {
  finalStandings: StandingsByPlayer;
  winnerPlayerId: string;
}

export interface RematchStatusEvent {
  readyCount: number;
  totalConnectedPlayers: number;
}

export interface RoomClosedEvent {
  reason: string;
}
