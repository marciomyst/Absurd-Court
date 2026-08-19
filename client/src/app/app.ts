import { Component, OnInit, inject } from '@angular/core';
import { HubEventBridgeService } from './core/realtime/hub-event-bridge.service';
import { GameStateService } from './core/state/game-state.service';
import { Home } from './features/room/home/home';
import { Join } from './features/room/join/join';
import { Lobby } from './features/room/lobby/lobby';
import { Case } from './features/match/case/case';
import { Waiting } from './features/room/waiting/waiting';
import { Verdict } from './features/match/verdict/verdict';
import { Ended } from './features/match/ended/ended';

@Component({
  selector: 'app-root',
  imports: [Home, Join, Lobby, Case, Waiting, Verdict, Ended],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  protected readonly gameState = inject(GameStateService);
  protected readonly hubEventBridge = inject(HubEventBridgeService);

  ngOnInit(): void {
    void this.gameState.bootstrap();
  }
}
