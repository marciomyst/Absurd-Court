using AbsurdCourt.Domain.Common;

namespace AbsurdCourt.Domain.Events;

public sealed record MatchStarted(Guid MatchId, Guid RoomId, int CaseCount) : IDomainEvent;

public sealed record RoundOpened(Guid MatchId, Guid RoomId, Guid RoundId, Guid CaseFileId, int OrderIndex, int CaseCount, DateTime DeadlineUtc) : IDomainEvent;

public sealed record DefenseSubmitted(Guid MatchId, Guid RoomId, Guid RoundId, Guid PlayerId, int FiledCount, int TotalPlayers) : IDomainEvent;

public sealed record RoundResult(Guid PlayerId, string DefenseText, bool WasSubmitted, string ParecerText, int Points);

public sealed record RoundClosed(Guid MatchId, Guid RoomId, Guid RoundId, int OrderIndex, int CaseCount, IReadOnlyList<RoundResult> Results, IReadOnlyDictionary<Guid, int> Standings) : IDomainEvent;

public sealed record MatchEnded(Guid MatchId, Guid RoomId, IReadOnlyDictionary<Guid, int> FinalStandings, Guid WinnerPlayerId) : IDomainEvent;
