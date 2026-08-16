import { Component, computed, inject, signal } from '@angular/core';
import { GameStateService } from '../../../core/state/game-state.service';
import { playerColor } from '../../../shared/player-color';
import * as QRCode from 'qrcode';

const CASE_OPTIONS = [3, 5, 10, 15];
const TIME_OPTIONS = [15, 30, 45, 60];

@Component({
  selector: 'app-lobby',
  imports: [],
  templateUrl: './lobby.html',
  styleUrl: './lobby.scss',
})
export class Lobby {
  protected readonly gameState = inject(GameStateService);

  readonly caseOptions = CASE_OPTIONS;
  readonly timeOptions = TIME_OPTIONS;
  readonly lockedCaseOptions = new Set([15]);
  readonly lockedTimeOptions = new Set([60]);

  readonly caseCount = computed(() => this.gameState.room()?.caseCount ?? 3);
  readonly roundDurationSeconds = computed(() => this.gameState.room()?.roundDurationSeconds ?? 15);

  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly qrCodeUrl = signal<string | null>(null);
  readonly playerSlots = Array.from({ length: 8 }, (_, index) => index);

  readonly players = computed(() => this.gameState.players());
  readonly canStart = computed(() => this.gameState.isHost() && this.players().length >= 2 && !this.busy());

  playerColor = playerColor;

  async showQrCode(): Promise<void> {
    const roomCode = this.gameState.room()?.roomCode;
    if (!roomCode) return;

    this.qrCodeUrl.set(await QRCode.toDataURL(roomCode, {
      width: 240,
      margin: 2,
      errorCorrectionLevel: 'M',
      color: { dark: '#151515', light: '#f5f0e6' },
    }));
  }

  closeQrCode(): void {
    this.qrCodeUrl.set(null);
  }

  async setCaseCount(value: number): Promise<void> {
    if (!this.gameState.isHost() || this.busy() || value === this.caseCount()) return;
    await this.applySettings(value, this.roundDurationSeconds());
  }

  async setRoundDuration(value: number): Promise<void> {
    if (!this.gameState.isHost() || this.busy() || value === this.roundDurationSeconds()) return;
    await this.applySettings(this.caseCount(), value);
  }

  private async applySettings(caseCount: number, roundDurationSeconds: number): Promise<void> {
    try {
      await this.gameState.updateSettings(caseCount, roundDurationSeconds);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Não foi possível atualizar as configurações.');
    }
  }

  async startMatch(): Promise<void> {
    if (!this.canStart()) return;
    this.busy.set(true);
    this.error.set(null);
    try {
      await this.gameState.startMatch();
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Não foi possível iniciar o julgamento.');
    } finally {
      this.busy.set(false);
    }
  }
}
