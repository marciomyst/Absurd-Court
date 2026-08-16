import { Injectable } from '@angular/core';

const STORAGE_ROOM_CODE = 'absurd-court:roomCode';
const LEGACY_STORAGE_RECONNECT_TOKEN = 'absurd-court:reconnectToken';
export interface PersistedPlayerSession {
  roomCode: string;
}

@Injectable({ providedIn: 'root' })
export class PlayerSessionService {
  read(): PersistedPlayerSession | null {
    localStorage.removeItem(LEGACY_STORAGE_RECONNECT_TOKEN);
    const roomCode = localStorage.getItem(STORAGE_ROOM_CODE);
    return roomCode ? { roomCode } : null;
  }

  save(roomCode: string): void {
    localStorage.setItem(STORAGE_ROOM_CODE, roomCode);
  }

  clear(): void {
    localStorage.removeItem(STORAGE_ROOM_CODE);
    localStorage.removeItem(LEGACY_STORAGE_RECONNECT_TOKEN);
  }
}
