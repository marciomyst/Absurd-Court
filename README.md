# Court of the Absurd

> Justice is blind. We are crazy.

Real-time multiplayer party game in the style of Jackbox/Quiplash: players receive absurd accusations and have a short time to write the most convincing (or funniest) defense possible. At the center of each round, a generative-AI judge reads the case file, compares every defense through a scoring rubric, and writes a distinct, deadpan judicial opinion for each player before determining the winner. After a few cases, the player with the most points wins.

Backend in **C# / ASP.NET Core / SignalR**, frontend in **Angular**.

## Demo

Try the live demo: [https://bit.ly/4wGCSJo](https://bit.ly/4wGCSJo)

Scan the QR Code to open it on another device:

<p align="center">
  <a href="https://bit.ly/4wGCSJo">
    <img src="docs/demo-qr-code.svg" width="240" alt="QR Code for the Court of the Absurd demo" />
  </a>
</p>

### How to play

1. One player creates a trial and shares the room code or QR Code.
2. The other players join, choose a name, and submit a defense before the timer ends.
3. The AI judge compares the defenses, issues individual opinions, and awards the round points.
4. The host advances through the cases; the highest total score wins the trial.

## Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core, SignalR, EF Core + SQLite, MediatR |
| Judge | Configurable provider (Anthropic/Claude Haiku 4.5, OpenAI, or Gemini) |
| Frontend | Angular 21 (standalone components, signals), `@microsoft/signalr` |
| Architecture | DDD + Vertical Slice in the backend (`Domain` → `Application` → `Infrastructure` → `Api`) |

## Repository structure

```
absurd-court-lite/
  server/
    AbsurdCourt.sln
    src/
      AbsurdCourt.Domain/          # Room and Match aggregates, value objects, domain events — zero external dependencies
      AbsurdCourt.Application/     # MediatR use cases (Features/Rooms, Features/Matches)
      AbsurdCourt.Infrastructure/  # EF Core, AI adapters, SignalR (CourtHub + notifier)
      AbsurdCourt.Api/             # composition root (Program.cs), round-deadline sweeper
    tests/
      AbsurdCourt.Domain.Tests/
      AbsurdCourt.Application.Tests/
  client/
    src/app/
      core/services/               # CourtHubService (SignalR) and GameStateService (global signal state)
      features/                    # home, join, lobby, case, waiting, verdict, ended
      shared/                      # TypeScript models, visual theme, utilities
```

## Running locally

### Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) or later
- [Node.js](https://nodejs.org/) 20+ and pnpm
- An API key for the selected provider (optional — without one, the judge automatically uses a fallback opinion and the game continues to work normally)

### 1. Backend

```bash
cd server/src/AbsurdCourt.Api

# optional: configure the Anthropic key (never committed; kept outside the repository with User Secrets)
dotnet user-secrets set Anthropic:ApiKey "sk-ant-..."

# alternatively, use OpenAI
dotnet user-secrets set AI:Provider "OpenAI"
dotnet user-secrets set OpenAI:ApiKey "sk-..."

# or use Gemini on the free tier with a Google AI Studio key
dotnet user-secrets set AI:Provider "Gemini"
dotnet user-secrets set Gemini:ApiKey "AIza..."

dotnet run
```

The API starts on `http://localhost:5122` (the `http` profile) or `https://localhost:7129` (the `https` profile). On the first run, it automatically applies database migrations and seeds approximately 15 example absurd cases — no manual database step is required.

### 2. Frontend

In another terminal:

```bash
cd client
pnpm install
pnpm start
```

Open `http://localhost:4200`. The API address is hard-coded in `client/src/app/core/services/court-hub.service.ts` (`HUB_URL`) — adjust it if you run the backend on another port.

### 3. Play

Open `http://localhost:4200` in two different tabs/devices: one creates the room ("Create new trial"), and the other joins using the displayed 6-digit code. At least 2 players are required to start.

## How the game works

1. **Room**: the host creates a room and selects the number of cases (3/5/10) and the duration of each round (30/45/60s).
2. **Case**: everyone receives the same absurd accusation and writes a defense (up to 200 characters) before time runs out.
3. **Deliberation**: when everyone files their defense (or time expires), the AI judges all defenses for that case at once, chooses a winner (+1000 points), and gives each player an individual, funny opinion.
4. **Next case / final ruling**: the host advances through the cases; at the end, the cumulative score determines the session winner, with an option for a rematch in the same room.

## Tests

```bash
cd server
dotnet test
```

48 tests cover aggregate invariants (`Domain.Tests`) and complete MediatR flows, including idempotent round closing under concurrency (`Application.Tests`).

## Configuration

- **Database**: SQLite by default (`server/src/AbsurdCourt.Api/appsettings.json`, `ConnectionStrings:Court`). To switch to PostgreSQL/SQL Server, replace the registered provider in `AbsurdCourt.Infrastructure/DependencyInjection.cs`.
- **Judge provider**: `AI:Provider` in `appsettings.json` (`Anthropic` by default; `OpenAI` and `Gemini` are also accepted). Each provider has its own options (`Anthropic:Model`, `OpenAI:Model`, or `Gemini:Model`). Gemini can use the Google AI Studio free tier, subject to the account's available limits and models.
- **API keys**: `appsettings.json` explicitly leaves `ApiKey` fields as `<FROM SECRETS>`. Override them using User Secrets or environment variables, for example: `Anthropic__ApiKey`, `OpenAI__ApiKey`, `Gemini__ApiKey`, and `AI__Provider`.
- **CORS**: enabled for `http://localhost:4200` in `Program.cs` — adjust it if the frontend runs at another address.

## Out of scope (by design)

- Authentication/user accounts — rooms are anonymous and ephemeral.
- Automatic host migration if the host disconnects during a match.
- Deployment/containerization (Docker, Azure, and so on).
- Voice/video, spectator mode, and authoring cases through the UI.

## Origin

Based on a navigable visual prototype ("Court of the Absurd") — this repository is the real, playable implementation, with a real-time backend and a genuine AI-powered judge.
