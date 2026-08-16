using AbsurdCourt.Domain.Events;

namespace AbsurdCourt.Application.Abstractions;

/// <summary>
/// The "tell clients what happened" port. Implemented by SignalRRoomNotifier in
/// Infrastructure and invoked by domain-event notification handlers there — Application
/// handlers never call this directly, they just let aggregates raise domain events.
/// </summary>
public interface IRoomNotifier
{
    Task PlayerJoinedAsync(Guid roomId, Guid playerId, string playerName, string initials, bool isHost, CancellationToken ct);
    Task PlayerReconnectedAsync(Guid roomId, Guid playerId, CancellationToken ct);
    Task PlayerDisconnectedAsync(Guid roomId, Guid playerId, CancellationToken ct);
    Task RoomSettingsUpdatedAsync(Guid roomId, int caseCount, int roundDurationSeconds, CancellationToken ct);
    Task MatchStartedAsync(Guid roomId, Guid matchId, int caseCount, CancellationToken ct);
    Task CaseStartedAsync(Guid roomId, Guid roundId, int caseNo, int caseTotal, string autos, string hint, DateTime deadlineUtc, CancellationToken ct);
    Task DefenseFiledAsync(Guid roomId, Guid playerId, int filedCount, int totalPlayers, CancellationToken ct);
    Task DeliberationReadyAsync(Guid roomId, IReadOnlyList<RoundResult> results, IReadOnlyDictionary<Guid, int> standings, CancellationToken ct);
    Task MatchEndedAsync(Guid roomId, IReadOnlyDictionary<Guid, int> finalStandings, Guid winnerPlayerId, CancellationToken ct);
    Task RematchStatusAsync(Guid roomId, int readyCount, int totalConnectedPlayers, CancellationToken ct);
    Task RoomClosedAsync(Guid roomId, string reason, CancellationToken ct);
}
