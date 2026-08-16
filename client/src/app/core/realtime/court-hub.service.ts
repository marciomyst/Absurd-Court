import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import type { CreateRoomResult, JoinRoomResult, RejoinResult } from '../../shared/models/room.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CourtHubService {
  private readonly connection = new signalR.HubConnectionBuilder()
    .withUrl(environment.hubUrl, { withCredentials: true })
    .withAutomaticReconnect()
    .build();

  readonly state = signal<signalR.HubConnectionState>(this.connection.state);

  private startPromise: Promise<void> | null = null;

  constructor() {
    this.connection.onreconnecting(() => this.state.set(this.connection.state));
    this.connection.onreconnected(() => {
      this.state.set(this.connection.state);
      this.reconnectedHandlers.forEach((h) => h());
    });
    this.connection.onclose(() => this.state.set(this.connection.state));
  }

  private readonly reconnectedHandlers: Array<() => void> = [];

  /**
   * withAutomaticReconnect() transparently re-establishes the transport, but the server
   * sees it as a brand new connection — a fresh ConnectionId means the old one's
   * Context.Items (room/player identity) and SignalR group membership are both gone.
   * Register a handler here to redo the Rejoin handshake once reconnected.
   */
  onReconnected(handler: () => void): void {
    this.reconnectedHandlers.push(handler);
  }

  /** Idempotent: safe to call from multiple components without racing duplicate start() calls. */
  async connect(): Promise<void> {
    if (this.connection.state === signalR.HubConnectionState.Connected) return;
    this.startPromise ??= this.connection
      .start()
      .then(() => this.state.set(this.connection.state))
      .catch((err) => {
        // A failed *first* connect (e.g. page loaded while the API was still starting)
        // must not permanently wedge future connect() calls behind this same rejection —
        // clear the cache so the next caller gets a fresh start() attempt.
        this.startPromise = null;
        throw err;
      });
    await this.startPromise;
  }

  on<T>(methodName: string, handler: (payload: T) => void): void {
    this.connection.on(methodName, handler);
  }

  off(methodName: string): void {
    this.connection.off(methodName);
  }

  createRoom(hostName: string): Promise<CreateRoomResult> {
    return this.connection.invoke<CreateRoomResult>('CreateRoom', hostName);
  }

  joinRoom(roomCode: string, playerName: string): Promise<JoinRoomResult> {
    return this.connection.invoke<JoinRoomResult>('JoinRoom', roomCode, playerName);
  }

  rejoin(roomCode: string): Promise<RejoinResult> {
    return this.connection.invoke<RejoinResult>('Rejoin', roomCode);
  }

  updateSettings(caseCount: number, roundDurationSeconds: number): Promise<void> {
    return this.connection.invoke('UpdateSettings', caseCount, roundDurationSeconds);
  }

  startMatch(): Promise<void> {
    return this.connection.invoke('StartMatch');
  }

  submitDefense(text: string): Promise<void> {
    return this.connection.invoke('SubmitDefense', text);
  }

  nextRound(): Promise<void> {
    return this.connection.invoke('NextRound');
  }

  requestRematch(wantsRematch: boolean): Promise<void> {
    return this.connection.invoke('RequestRematch', wantsRematch);
  }
}
