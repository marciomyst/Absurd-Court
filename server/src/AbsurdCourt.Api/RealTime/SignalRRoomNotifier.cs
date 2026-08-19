using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Events;
using Microsoft.AspNetCore.SignalR;

namespace AbsurdCourt.Api.RealTime;

public sealed class SignalRRoomNotifier(IHubContext<CourtHub> hub) : IRoomNotifier
{
    public Task PlayerJoinedAsync(Guid roomId, Guid playerId, string playerName, string initials, bool isHost, CancellationToken ct) =>
        Group(roomId).SendAsync("PlayerJoined", new { playerId, playerName, initials, isHost }, ct);

    public Task PlayerReconnectedAsync(Guid roomId, Guid playerId, CancellationToken ct) =>
        Group(roomId).SendAsync("PlayerReconnected", new { playerId }, ct);

    public Task PlayerDisconnectedAsync(Guid roomId, Guid playerId, CancellationToken ct) =>
        Group(roomId).SendAsync("PlayerDisconnected", new { playerId }, ct);

    public Task PlayerLeftAsync(Guid roomId, Guid playerId, CancellationToken ct) =>
        Group(roomId).SendAsync("PlayerLeft", new { playerId }, ct);

    public Task RoomSettingsUpdatedAsync(Guid roomId, int caseCount, int roundDurationSeconds, CancellationToken ct) =>
        Group(roomId).SendAsync("RoomSettingsUpdated", new { caseCount, roundDurationSeconds }, ct);

    public Task MatchStartedAsync(Guid roomId, Guid matchId, int caseCount, CancellationToken ct) =>
        Group(roomId).SendAsync("MatchStarted", new { matchId, caseCount }, ct);

    public Task CaseStartedAsync(Guid roomId, Guid roundId, int caseNo, int caseTotal, string autos, string hint, DateTime deadlineUtc, CancellationToken ct) =>
        Group(roomId).SendAsync("CaseStarted", new { roundId, caseNo, caseTotal, autos, hint, deadlineUtc }, ct);

    public Task DefenseFiledAsync(Guid roomId, Guid playerId, int filedCount, int totalPlayers, CancellationToken ct) =>
        Group(roomId).SendAsync("DefenseFiled", new { playerId, filedCount, totalPlayers }, ct);

    public Task DeliberationReadyAsync(Guid roomId, IReadOnlyList<RoundResult> results, IReadOnlyDictionary<Guid, int> standings, CancellationToken ct) =>
        Group(roomId).SendAsync("DeliberationReady", new { results, standings }, ct);

    public Task MatchEndedAsync(Guid roomId, IReadOnlyDictionary<Guid, int> finalStandings, Guid winnerPlayerId, CancellationToken ct) =>
        Group(roomId).SendAsync("MatchEnded", new { finalStandings, winnerPlayerId }, ct);

    public Task RematchStatusAsync(Guid roomId, int readyCount, int totalConnectedPlayers, CancellationToken ct) =>
        Group(roomId).SendAsync("RematchStatus", new { readyCount, totalConnectedPlayers }, ct);

    public Task RoomClosedAsync(Guid roomId, string reason, CancellationToken ct) =>
        Group(roomId).SendAsync("RoomClosed", new { reason }, ct);

    private IClientProxy Group(Guid roomId) => hub.Clients.Group(GroupNames.ForRoom(roomId));
}
