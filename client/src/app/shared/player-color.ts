// Deterministic per-player color, since (unlike the original mockup's 4 hardcoded
// players) real rooms have up to 8 players whose ids aren't known ahead of time.
const PALETTE = [
  'linear-gradient(160deg,#e3c378,#b98f3c)',
  'linear-gradient(160deg,#8c93b8,#4e5573)',
  'linear-gradient(160deg,#c9a9a0,#8a5f55)',
  'linear-gradient(160deg,#9eb8a8,#4f6f60)',
  'linear-gradient(160deg,#c3879e,#7a4256)',
  'linear-gradient(160deg,#7fb0c9,#3f6b7d)',
  'linear-gradient(160deg,#c9b37f,#7d6a3f)',
  'linear-gradient(160deg,#a99ccf,#5c4f80)',
];

export function playerColor(playerId: string): string {
  return PALETTE[hashString(playerId) % PALETTE.length];
}

function hashString(value: string): number {
  let hash = 0;
  for (let i = 0; i < value.length; i++) {
    hash = (hash * 31 + value.charCodeAt(i)) >>> 0;
  }
  return hash;
}
