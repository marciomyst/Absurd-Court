export type RoundStatus = 'Open' | 'Judging' | 'Revealed';

/// Guid-keyed Dictionary<Guid,int> on the backend serializes as a plain JSON object with
/// stringified-guid keys — playerId is always the key, score/points the value.
export type StandingsByPlayer = Record<string, number>;

export interface RoundResultDto {
  playerId: string;
  defenseText: string;
  wasSubmitted: boolean;
  parecerText: string;
  points: number;
}

export interface MatchSnapshotDto {
  matchId: string;
  caseCount: number;
  currentRoundIndex: number;
  roundStatus: RoundStatus;
  deadlineUtc: string | null;
  autos: string | null;
  hint: string | null;
  filedPlayerIds: string[];
  standings: StandingsByPlayer;
  /** Populated only when roundStatus is 'Revealed' — lets a client that reconnected mid-verdict render it instead of getting stuck on "waiting". */
  results: RoundResultDto[] | null;
}
