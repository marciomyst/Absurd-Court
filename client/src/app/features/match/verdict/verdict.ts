import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { GameStateService } from '../../../core/state/game-state.service';
import { playerColor } from '../../../shared/player-color';
import type { RoundResultDto } from '../../../shared/models/match.model';

@Component({
  selector: 'app-verdict',
  imports: [],
  templateUrl: './verdict.html',
  styleUrl: './verdict.scss',
})
export class Verdict implements OnDestroy {
  protected readonly gameState = inject(GameStateService);
  playerColor = playerColor;

  readonly round = computed(() => this.gameState.currentRound());
  readonly results = computed(() => this.gameState.deliberationResults());
  readonly players = computed(() => this.gameState.players());
  readonly isHost = this.gameState.isHost;

  readonly winnerResult = computed<RoundResultDto | null>(() => {
    const results = this.results();
    if (results.length === 0) return null;
    return results.reduce((best, r) => (r.points > best.points ? r : best), results[0]);
  });

  readonly otherResults = computed(() => {
    const winner = this.winnerResult();
    return this.results().filter((r) => r.playerId !== winner?.playerId);
  });

  readonly isLastCase = computed(() => {
    const round = this.round();
    return !!round && round.caseNo >= round.caseTotal;
  });

  readonly openPlayerId = signal<string | null>(null);
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly remainingSeconds = signal(60);
  private countdownTimer?: number;

  constructor() {
    this.startCountdown();
  }

  toggleOther(playerId: string): void {
    this.openPlayerId.set(this.openPlayerId() === playerId ? null : playerId);
  }

  playerName(playerId: string): string {
    return this.players().find((p) => p.playerId === playerId)?.name ?? '???';
  }

  playerInitials(playerId: string): string {
    return this.players().find((p) => p.playerId === playerId)?.initials ?? '??';
  }

  async advance(): Promise<void> {
    if (!this.isHost() || this.busy()) return;
    this.stopCountdown();
    this.busy.set(true);
    this.error.set(null);
    try {
      await this.gameState.nextRound();
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Não foi possível avançar.');
    } finally {
      this.busy.set(false);
    }
  }

  ngOnDestroy(): void {
    this.stopCountdown();
  }

  private startCountdown(): void {
    this.stopCountdown();
    this.remainingSeconds.set(60);
    this.countdownTimer = window.setInterval(() => {
      const next = this.remainingSeconds() - 1;
      this.remainingSeconds.set(next);
      if (next <= 0) {
        this.stopCountdown();
        if (this.isHost()) void this.advance();
      }
    }, 1000);
  }

  private stopCountdown(): void {
    if (this.countdownTimer !== undefined) window.clearInterval(this.countdownTimer);
    this.countdownTimer = undefined;
  }
}
