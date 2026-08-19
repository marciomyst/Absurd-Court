namespace AbsurdCourt.Application.Contracts;

public sealed record PlayerDto(Guid PlayerId, string Name, string Initials, bool IsHost, bool IsConnected);

public sealed record RoomSnapshotDto(
    Guid RoomId,
    string RoomCode,
    string Status,
    Guid HostPlayerId,
    IReadOnlyList<PlayerDto> Players,
    int CaseCount,
    int RoundDurationSeconds);

public sealed record RoundResultDto(Guid PlayerId, string DefenseText, bool WasSubmitted, string ParecerText, int Points);

public sealed record MatchSnapshotDto(
    Guid MatchId,
    int CaseCount,
    int CurrentRoundIndex,
    string RoundStatus,
    DateTime? DeadlineUtc,
    string? Autos,
    string? Hint,
    IReadOnlyList<Guid> FiledPlayerIds,
    IReadOnlyDictionary<Guid, int> Standings,
    /// <summary>Populated only when RoundStatus is Revealed — lets a client that reconnected mid-verdict render it instead of getting stuck on "waiting".</summary>
    IReadOnlyList<RoundResultDto>? Results);
