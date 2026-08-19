import { Component, computed, inject, signal } from '@angular/core';
import { GameStateService } from '../../../core/state/game-state.service';
import { playerColor } from '../../../shared/player-color';

@Component({
  selector: 'app-ended',
  imports: [],
  templateUrl: './ended.html',
  styleUrl: './ended.scss',
})
export class Ended {
  protected readonly gameState = inject(GameStateService);
  playerColor = playerColor;

  readonly players = computed(() => this.gameState.players());
  readonly standings = computed(() => this.gameState.finalStandings());
  readonly readyCount = this.gameState.rematchReadyCount;

  readonly ranked = computed(() =>
    this.players()
      .map((p) => ({ player: p, points: this.standings()[p.playerId] ?? 0 }))
      .sort((a, b) => b.points - a.points),
  );

  readonly iAmReady = signal(false);
  readonly busy = signal(false);

  async toggleReady(): Promise<void> {
    if (this.busy()) return;
    this.busy.set(true);
    try {
      const next = !this.iAmReady();
      await this.gameState.requestRematch(next);
      this.iAmReady.set(next);
    } finally {
      this.busy.set(false);
    }
  }

  async leaveRoom(): Promise<void> {
    await this.gameState.goHome();
  }
}
