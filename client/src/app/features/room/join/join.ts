import { Component, ElementRef, OnDestroy, ViewChild, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GameStateService } from '../../../core/state/game-state.service';
import { ABSURD_PARTY_NAMES } from '../home/home';

declare const BarcodeDetector: new (options?: { formats?: string[] }) => {
  detect(source: HTMLVideoElement): Promise<Array<{ rawValue?: string }>>;
};

@Component({
  selector: 'app-join',
  imports: [FormsModule],
  templateUrl: './join.html',
  styleUrl: './join.scss',
})
export class Join implements OnDestroy {
  private readonly gameState = inject(GameStateService);

  @ViewChild('scannerVideo') private scannerVideo?: ElementRef<HTMLVideoElement>;
  private cameraStream?: MediaStream;
  private scanTimer?: number;

  name = '';
  code = '';
  readonly placeholder = `ex. ${ABSURD_PARTY_NAMES[Math.floor(Math.random() * ABSURD_PARTY_NAMES.length)]}`;
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly scannerOpen = signal(false);

  get formattedInputCode(): string {
    return [this.code.slice(0, 4), this.code.slice(4, 8), this.code.slice(8, 12)]
      .filter(Boolean)
      .join('-');
  }

  get ready(): boolean {
    return this.code.length === 12 && this.name.trim().length > 0;
  }

  onCodeInput(input: HTMLInputElement): void {
    this.updateCodeInput(input, input.value, input.selectionStart ?? input.value.length);
  }

  onCodePaste(event: ClipboardEvent): void {
    event.preventDefault();
    const input = event.currentTarget as HTMLInputElement;
    const pastedValue = event.clipboardData?.getData('text') ?? '';
    this.updateCodeInput(input, pastedValue, pastedValue.length);
  }

  private updateCodeInput(input: HTMLInputElement, value: string, cursorSourcePosition: number): void {
    const validCharactersBeforeCursor = value
      .slice(0, cursorSourcePosition)
      .toUpperCase()
      .replace(/[^0-9A-F]/g, '');

    this.code = value.toUpperCase().replace(/[^0-9A-F]/g, '').slice(0, 12);
    input.value = this.formattedInputCode;

    const cursorPosition = this.formattedInputCode.slice(0, validCharactersBeforeCursor.length).length;
    input.setSelectionRange(cursorPosition, cursorPosition);
  }

  async openScanner(): Promise<void> {
    this.error.set(null);

    if (!('BarcodeDetector' in globalThis) || !navigator.mediaDevices?.getUserMedia) {
      this.error.set('A leitura pela câmera não é compatível neste navegador. Informe o código manualmente.');
      return;
    }

    this.scannerOpen.set(true);
    await new Promise<void>((resolve) => setTimeout(resolve));

    try {
      this.cameraStream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: { ideal: 'environment' } },
        audio: false,
      });
      const video = this.scannerVideo?.nativeElement;
      if (!video) return;
      video.srcObject = this.cameraStream;
      await video.play();
      this.scanFrame(video);
    } catch {
      this.closeScanner();
      this.error.set('Não foi possível acessar a câmera. Verifique a permissão ou informe o código manualmente.');
    }
  }

  closeScanner(): void {
    if (this.scanTimer !== undefined) window.clearTimeout(this.scanTimer);
    this.scanTimer = undefined;
    this.cameraStream?.getTracks().forEach((track) => track.stop());
    this.cameraStream = undefined;
    this.scannerOpen.set(false);
  }

  private scanFrame(video: HTMLVideoElement): void {
    if (!this.scannerOpen()) return;
    const detector = new BarcodeDetector({ formats: ['qr_code'] });
    detector.detect(video).then((codes) => {
      const value = codes[0]?.rawValue;
      if (value) {
        this.code = value.toUpperCase().replace(/[^0-9A-F]/g, '').slice(0, 12);
        this.closeScanner();
        return;
      }
      this.scanTimer = window.setTimeout(() => this.scanFrame(video), 180);
    }).catch(() => {
      this.scanTimer = window.setTimeout(() => this.scanFrame(video), 300);
    });
  }

  async enterRoom(): Promise<void> {
    if (!this.ready || this.busy()) return;
    this.busy.set(true);
    this.error.set(null);
    try {
      const roomCode = `${this.code.slice(0, 4)}-${this.code.slice(4, 8)}-${this.code.slice(8, 12)}`;
      await this.gameState.joinRoom(roomCode, this.name.trim());
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Não foi possível entrar na sala.');
    } finally {
      this.busy.set(false);
    }
  }

  goBack(): void {
    this.closeScanner();
    this.gameState.showHome();
  }

  ngOnDestroy(): void {
    this.closeScanner();
  }
}
