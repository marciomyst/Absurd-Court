import { Component, computed, inject } from '@angular/core';
import { GameStateService } from '../../../core/state/game-state.service';
import { playerColor } from '../../../shared/player-color';

@Component({
  selector: 'app-waiting',
  imports: [],
  templateUrl: './waiting.html',
  styleUrl: './waiting.scss',
})
export class Waiting {
  protected readonly gameState = inject(GameStateService);
  playerColor = playerColor;

  readonly round = computed(() => this.gameState.currentRound());
  readonly filedIds = computed(() => this.gameState.filedPlayerIds());
  readonly players = computed(() => this.gameState.players());

  readonly allFiled = computed(() => {
    const players = this.players();
    return players.length > 0 && players.every((p) => this.filedIds().includes(p.playerId));
  });

  readonly rows = computed(() =>
    this.players().map((p) => ({
      player: p,
      filed: this.filedIds().includes(p.playerId),
    })),
  );

  constructor() {
    // A client may miss DeliberationReady while its tab is suspended or its
    // connection is recovering. The snapshot is authoritative and repairs the
    // screen without requiring a manual refresh.
    void this.gameState.synchronize();
  }
}
