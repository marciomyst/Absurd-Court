import { Injectable, signal } from '@angular/core';

export type Screen = 'home' | 'join' | 'lobby' | 'case' | 'waiting' | 'verdict' | 'ended';

@Injectable({ providedIn: 'root' })
export class ScreenStateService {
  private readonly state = signal<Screen>('home');
  readonly current = this.state.asReadonly();

  showHome(): void { this.state.set('home'); }
  showJoin(): void { this.state.set('join'); }
  showLobby(): void { this.state.set('lobby'); }
  showCase(): void { this.state.set('case'); }
  showWaiting(): void { this.state.set('waiting'); }
  showVerdict(): void { this.state.set('verdict'); }
  showEnded(): void { this.state.set('ended'); }
}
