import type { MatchSnapshotDto } from './match.model';

export interface PlayerDto {
  playerId: string;
  name: string;
  initials: string;
  isHost: boolean;
  isConnected: boolean;
}

export type RoomStatus = 'Lobby' | 'InProgress' | 'Ended';

export interface RoomSnapshotDto {
  roomId: string;
  roomCode: string;
  status: RoomStatus;
  hostPlayerId: string;
  players: PlayerDto[];
  caseCount: number;
  roundDurationSeconds: number;
}

export interface CreateRoomResult {
  room: RoomSnapshotDto;
  yourPlayerId: string;
}

export interface JoinRoomResult {
  room: RoomSnapshotDto;
  yourPlayerId: string;
}

export interface RejoinResult {
  room: RoomSnapshotDto;
  yourPlayerId: string;
  match: MatchSnapshotDto | null;
}
