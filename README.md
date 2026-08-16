# Tribunal do Absurdo

> A justiça é cega. Nós somos loucos.

Jogo de festa multiplayer em tempo real, estilo Jackbox/Quiplash: jogadores recebem acusações absurdas e têm um tempo curto pra escrever a defesa mais convincente (ou engraçada) possível. Um juiz movido a IA delibera, escolhe um vencedor por caso e distribui pontos. Depois de alguns casos, vence quem tiver mais pontos.

Backend em **C# / ASP.NET Core / SignalR**, frontend em **Angular**.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10, ASP.NET Core, SignalR, EF Core + SQLite, MediatR |
| Juiz | Provider configurável (Anthropic/Claude Haiku 4.5, OpenAI ou Gemini) |
| Frontend | Angular 21 (standalone components, signals), `@microsoft/signalr` |
| Arquitetura | DDD + Vertical Slice no backend (`Domain` → `Application` → `Infrastructure` → `Api`) |

## Estrutura do repositório

```
absurd-court-lite/
  server/
    AbsurdCourt.sln
    src/
      AbsurdCourt.Domain/          # aggregates Room e Match, value objects, eventos de domínio — zero dependências externas
      AbsurdCourt.Application/     # casos de uso via MediatR (Features/Rooms, Features/Matches)
      AbsurdCourt.Infrastructure/  # EF Core, adaptadores de IA, SignalR (CourtHub + notifier)
      AbsurdCourt.Api/             # composition root (Program.cs), sweeper de prazo de rodada
    tests/
      AbsurdCourt.Domain.Tests/
      AbsurdCourt.Application.Tests/
  client/
    src/app/
      core/services/               # CourtHubService (SignalR) e GameStateService (estado global em signals)
      features/                    # home, join, lobby, case, waiting, verdict, ended
      shared/                      # modelos TS, tema visual, utilitários
```

## Como rodar localmente

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) ou superior
- [Node.js](https://nodejs.org/) 20+ e pnpm
- Uma chave de API do provider escolhido (opcional — sem ela, o juiz usa um parecer de reserva automaticamente e o jogo continua funcionando normalmente)

### 1. Backend

```bash
cd server/src/AbsurdCourt.Api

# opcional: configure a chave Anthropic (nunca commitada, fica fora do repo via User Secrets)
dotnet user-secrets set Anthropic:ApiKey "sk-ant-..."

# alternativamente, use OpenAI
dotnet user-secrets set AI:Provider "OpenAI"
dotnet user-secrets set OpenAI:ApiKey "sk-..."

# ou use Gemini no free tier via uma chave do Google AI Studio
dotnet user-secrets set AI:Provider "Gemini"
dotnet user-secrets set Gemini:ApiKey "AIza..."

dotnet run
```

A API sobe em `http://localhost:5122` (perfil `http`) ou `https://localhost:7129` (perfil `https`). Na primeira execução, aplica as migrações do banco automaticamente e semeia ~15 casos absurdos de exemplo — nenhum passo manual de banco de dados é necessário.

### 2. Frontend

Em outro terminal:

```bash
cd client
pnpm install
pnpm start
```

Abre em `http://localhost:4200`. O endereço da API está fixado em `client/src/app/core/services/court-hub.service.ts` (`HUB_URL`) — ajuste se você rodar o backend em outra porta.

### 3. Jogar

Abra `http://localhost:4200` em duas abas/dispositivos diferentes: uma cria a sala ("Criar novo julgamento"), a outra entra com o código de 6 dígitos exibido. É necessário pelo menos 2 jogadores pra iniciar.

## Como funciona o jogo

1. **Sala**: o host cria a sala e escolhe quantos casos (3/5/10) e a duração de cada rodada (30/45/60s).
2. **Caso**: todos recebem a mesma acusação absurda e escrevem uma defesa (até 200 caracteres) antes do tempo acabar.
3. **Deliberação**: quando todos protocolam (ou o tempo esgota), a IA julga todas as defesas daquele caso de uma vez, escolhe uma vencedora (+1000 pontos) e dá um parecer individual e cômico pra cada jogador.
4. **Próximo caso / Sentença final**: o host avança pros próximos casos; ao final, o placar acumulado decide o vencedor da sessão, com opção de revanche na mesma sala.

## Testes

```bash
cd server
dotnet test
```

48 testes cobrindo as invariantes dos aggregates (`Domain.Tests`) e os fluxos completos via MediatR, incluindo a idempotência do fechamento de rodada sob concorrência (`Application.Tests`).

## Configuração

- **Banco de dados**: SQLite por padrão (`server/src/AbsurdCourt.Api/appsettings.json`, chave `ConnectionStrings:Court`). Trocar para PostgreSQL/SQL Server é só trocar o provider registrado em `AbsurdCourt.Infrastructure/DependencyInjection.cs`.
- **Provider do juiz**: `AI:Provider` em `appsettings.json` (`Anthropic` por padrão; também aceita `OpenAI` e `Gemini`). Cada provider tem suas próprias opções (`Anthropic:Model`, `OpenAI:Model` ou `Gemini:Model`). O Gemini pode usar o free tier do Google AI Studio, sujeito aos limites e modelos disponíveis na conta.
- **Chaves de API**: `appsettings.json` deixa explicitamente os campos `ApiKey` como `<FROM SECRETS>`. Sobrescreva-os por User Secrets ou variáveis de ambiente, por exemplo `Anthropic__ApiKey`, `OpenAI__ApiKey`, `Gemini__ApiKey` e `AI__Provider`.
- **CORS**: liberado para `http://localhost:4200` em `Program.cs` — ajuste se o frontend rodar em outro endereço.

## Fora do escopo (por decisão consciente)

- Autenticação/contas de usuário — salas são anônimas e efêmeras.
- Migração automática de host caso ele se desconecte no meio da partida.
- Deploy/containerização (Docker, Azure etc.).
- Voz/vídeo, modo espectador, autoria de casos pela interface.

## Origem

Baseado em um protótipo visual navegável ("Tribunal do Absurdo") — este repositório é a implementação real e jogável, com backend em tempo real e juiz movido a IA de verdade.
