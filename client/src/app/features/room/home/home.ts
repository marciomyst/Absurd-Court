import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GameStateService } from '../../../core/state/game-state.service';

export const ABSURD_PARTY_NAMES = [
  'Iara do Wi-Fi',
  'Clóvis Inocente',
  'Dona Planilha',
  'Zé Ninguém',
  'Gilberto do Elevador',
  'Testemunha Suspeita',
  'Marlene do Grupo',
  'Réu do Biscoito',
  'Valdisnei Jurídico',
  'Cidadão em Recursos',
  'Tia do Crochê',
  'Doutor Batatinha',
  'Patrícia Sem Contexto',
  'Pato de Borracha',
  'A Pessoa que Clicou',
  'Jurandir dos Áudios',
  'Réu Confuso Ltda.',
  'Agente do Caos',
  'Vossa Excelência',
  'O Último a Saber',
  'Culpado por Engano',
  'Testemunha em Obras',
  'Condenado pelo Bom Senso',
  'Fulano Associado',
  'Márcia do Café',
  'Osvaldo do Caps Lock',
  'Beto do Guarda-Chuva',
  'A Parte Contrária',
  'Célia do Quase',
  'Rogério do Protocolo',
  'Mestre do Atalho',
  'Naná da Evidência',
  'O Primo da Testemunha',
  'Leôncio Suspeito',
  'Diva do Fórum',
  'Everaldo Sem Senha',
  'A Dona da Razão',
  'Jurema do Carimbo',
  'Pedro Improviso',
  'O Homem do Terno',
  'Sandra da Reunião',
  'Réu de Meia-Tigela',
  'Caio do Contexto',
  'A Voz do Além',
  'Bárbara Incidental',
  'O Vizinho do 302',
  'Nestor do Rodapé',
  'Lúcia do Parágrafo',
  'Moisés da Impressora',
  'O Caso Encerrado',
  'Neide da Notificação',
  'Juvenal do Anexo',
  'A Parte Mais Fraca',
  'Rita do Recurso',
  'O Dono do Pão de Queijo',
  'Mário do Elevador',
  'Sueli da Ata',
  'O Réu Presencial',
  'Djalma do Drama',
  'A Última Instância',
  'Kátia do Print',
  'O Advogado do Sofá',
  'Noêmia do Nevoeiro',
  'Fábio Sem Testemunha',
  'A Prova Material',
  'Tadeu do Cafezinho',
  'O Acusado Anônimo',
  'Cris da Contradição',
  'A Senhora do Carimbo',
  'Jonas do Anexo Dois',
  'O Réu de Domingo',
  'Mônica do Plural',
  'O Perito em Desculpas',
  'A Testemunha Principal',
  'Geraldo do Parêntese',
  'Laila da Liminar',
  'O Especialista em Nada',
  'Benedito do Rodapé',
  'A Parte Interessada',
  'Marta do Ctrl Z',
  'O Juiz do Sofá',
  'Sérgio da Suspeita',
  'A Pessoa do Guarda-Chuva',
  'Otávio do Incidente',
  'Cida da Citação',
  'O Réu de Última Hora',
  'Vera da Verdade',
  'O Doutor em Desculpas',
  'Madalena do Mandado',
  'A Prova que Sumiu',
  'Wilson da Vírgula',
  'O Vizinho Inocentíssimo',
  'Dalva do Documento',
  'A Parte Sem Álibi',
  'Nivaldo do Notebook',
  'O Acusado do Café',
  'Tereza da Tréplica',
  'A Testemunha Offline',
  'Roberto do Regulamento',
  'O Réu em Férias',
  'Carmem do Contraditório',
  'A Última Testemunha',
];

@Component({
  selector: 'app-home',
  imports: [FormsModule],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private readonly gameState = inject(GameStateService);

  name = '';
  readonly placeholder = `ex. ${ABSURD_PARTY_NAMES[Math.floor(Math.random() * ABSURD_PARTY_NAMES.length)]}`;
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  async createRoom(): Promise<void> {
    if (!this.name.trim() || this.busy()) return;
    this.busy.set(true);
    this.error.set(null);
    try {
      await this.gameState.createRoom(this.name.trim());
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Não foi possível criar a sala.');
    } finally {
      this.busy.set(false);
    }
  }

  goJoin(): void {
    this.gameState.goJoin();
  }
}
