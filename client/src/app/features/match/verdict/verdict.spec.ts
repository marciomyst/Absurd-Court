import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { GameStateService } from '../../../core/state/game-state.service';
import { Verdict } from './verdict';

describe('Verdict', () => {
  afterEach(() => vi.useRealTimers());

  it('advances the round automatically after sixty seconds when the current player is the host', async () => {
    vi.useFakeTimers();
    const nextRound = vi.fn().mockResolvedValue(undefined);
    const gameState = {
      currentRound: signal({
        roundId: 'round-1',
        caseNo: 1,
        caseTotal: 3,
        autos: 'Autos de teste',
        hint: 'Dica de teste',
        deadlineUtc: new Date().toISOString(),
      }),
      deliberationResults: signal([]),
      players: signal([]),
      isHost: signal(true),
      nextRound,
    } as unknown as GameStateService;

    await TestBed.configureTestingModule({
      imports: [Verdict],
      providers: [{ provide: GameStateService, useValue: gameState }],
    }).compileComponents();

    const fixture = TestBed.createComponent(Verdict);
    fixture.detectChanges();

    await vi.advanceTimersByTimeAsync(60_000);

    expect(nextRound).toHaveBeenCalledTimes(1);
    fixture.destroy();
  });
});
