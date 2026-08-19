import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GameStateService } from '../../../core/state/game-state.service';

const MAX_LENGTH = 200;
const TICK_MS = 250;

@Component({
  selector: 'app-case',
  imports: [FormsModule],
  templateUrl: './case.html',
  styleUrl: './case.scss',
})
export class Case implements OnInit {
  protected readonly gameState = inject(GameStateService);
  private readonly destroyRef = inject(DestroyRef);

  defense = '';
  readonly busy = signal(false);
  readonly nowMs = signal(Date.now());
  private submitted = false;

  readonly maxLength = MAX_LENGTH;

  readonly round = computed(() => this.gameState.currentRound());
  readonly totalSeconds = computed(() => this.gameState.room()?.roundDurationSeconds ?? 60);

  readonly remainingSeconds = computed(() => {
    const round = this.round();
    if (!round) return 0;
    const deadline = new Date(round.deadlineUtc).getTime();
    return Math.max(0, Math.round((deadline - this.nowMs()) / 1000));
  });

  readonly percentRemaining = computed(() => {
    const total = this.totalSeconds();
    return total > 0 ? Math.max(0, Math.min(100, Math.round((this.remainingSeconds() / total) * 100))) : 0;
  });

  readonly isLow = computed(() => this.remainingSeconds() <= 10);

  ngOnInit(): void {
    const interval = setInterval(() => {
      this.nowMs.set(Date.now());
      if (this.remainingSeconds() <= 0 && !this.submitted) {
        this.submitted = true;
        if (this.defense.trim()) {
          void this.doSubmit();
        } else {
          // Nothing written — don't send an empty defense (the backend rejects it).
          // Just stop staring at a dead countdown; the round closes without our
          // submission and we get "revelia" once the deadline sweeper/others catch up.
          this.gameState.showWaiting();
        }
      }
    }, TICK_MS);
    this.destroyRef.onDestroy(() => clearInterval(interval));
  }

  onDefenseInput(value: string): void {
    this.defense = value.slice(0, MAX_LENGTH);
  }

  async submit(): Promise<void> {
    if (this.submitted || this.busy() || !this.defense.trim()) return;
    this.submitted = true;
    await this.doSubmit();
  }

  private async doSubmit(): Promise<void> {
    this.busy.set(true);
    try {
      await this.gameState.submitDefense(this.defense.trim());
    } finally {
      this.busy.set(false);
    }
  }
}
