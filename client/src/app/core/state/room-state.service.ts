import { Injectable, computed, signal } from '@angular/core';
import type { PlayerDto, RoomSnapshotDto } from '../../shared/models/room.model';

@Injectable({ providedIn: 'root' })
export class RoomStateService {
  readonly room = signal<RoomSnapshotDto | null>(null);
  readonly yourPlayerId = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly players = computed(() => this.room()?.players ?? []);
  readonly you = computed(() => this.players().find((p) => p.playerId === this.yourPlayerId()));
  readonly isHost = computed(() => !!this.you()?.isHost);

  applyIdentity(room: RoomSnapshotDto, playerId: string): void {
    this.room.set(room);
    this.yourPlayerId.set(playerId);
  }

  clear(): void {
    this.room.set(null);
    this.yourPlayerId.set(null);
  }

  updatePlayer(playerId: string, patch: Partial<PlayerDto>): void {
    const room = this.room();
    if (!room) return;
    this.room.set({
      ...room,
      players: room.players.map((p) => (p.playerId === playerId ? { ...p, ...patch } : p)),
    });
  }

  addPlayer(player: PlayerDto): void {
    const room = this.room();
    if (!room || room.players.some((p) => p.playerId === player.playerId)) return;
    this.room.set({ ...room, players: [...room.players, player] });
  }

  removePlayer(playerId: string): void {
    const room = this.room();
    if (!room) return;
    this.room.set({ ...room, players: room.players.filter((player) => player.playerId !== playerId) });
  }

  updateSettings(caseCount: number, roundDurationSeconds: number): void {
    const room = this.room();
    if (!room) return;
    this.room.set({ ...room, caseCount, roundDurationSeconds });
  }

  setError(message: string | null): void { this.errorMessage.set(message); }
  clearError(): void { this.errorMessage.set(null); }
}
