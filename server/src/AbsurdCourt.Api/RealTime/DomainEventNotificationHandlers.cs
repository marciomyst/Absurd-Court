using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Events;
using AbsurdCourt.Infrastructure.Persistence;
using MediatR;

namespace AbsurdCourt.Api.RealTime;

/// <summary>
/// One handler per Domain event, each just translating it into the matching IRoomNotifier
/// call. Kept together in one file since each is a couple of lines — the interesting logic
/// lives in the aggregates that raised these events, not here.
/// </summary>
public sealed class PlayerJoinedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<PlayerJoined>>
{
    public Task Handle(DomainEventNotification<PlayerJoined> n, CancellationToken ct)
    {
        var e = n.DomainEvent;
        return notifier.PlayerJoinedAsync(e.RoomId, e.PlayerId, e.PlayerName, e.Initials, e.IsHost, ct);
    }
}

public sealed class PlayerReconnectedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<PlayerReconnected>>
{
    public Task Handle(DomainEventNotification<PlayerReconnected> n, CancellationToken ct) =>
        notifier.PlayerReconnectedAsync(n.DomainEvent.RoomId, n.DomainEvent.PlayerId, ct);
}

public sealed class PlayerDisconnectedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<PlayerDisconnected>>
{
    public Task Handle(DomainEventNotification<PlayerDisconnected> n, CancellationToken ct) =>
        notifier.PlayerDisconnectedAsync(n.DomainEvent.RoomId, n.DomainEvent.PlayerId, ct);
}

public sealed class PlayerLeftHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<PlayerLeft>>
{
    public Task Handle(DomainEventNotification<PlayerLeft> n, CancellationToken ct) =>
        notifier.PlayerLeftAsync(n.DomainEvent.RoomId, n.DomainEvent.PlayerId, ct);
}

public sealed class RoomSettingsChangedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<RoomSettingsChanged>>
{
    public Task Handle(DomainEventNotification<RoomSettingsChanged> n, CancellationToken ct) =>
        notifier.RoomSettingsUpdatedAsync(n.DomainEvent.RoomId, n.DomainEvent.CaseCount, n.DomainEvent.RoundDurationSeconds, ct);
}

public sealed class RematchStatusChangedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<RematchStatusChanged>>
{
    public Task Handle(DomainEventNotification<RematchStatusChanged> n, CancellationToken ct) =>
        notifier.RematchStatusAsync(n.DomainEvent.RoomId, n.DomainEvent.ReadyCount, n.DomainEvent.TotalConnectedPlayers, ct);
}

public sealed class RoomClosedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<RoomClosed>>
{
    public Task Handle(DomainEventNotification<RoomClosed> n, CancellationToken ct) =>
        notifier.RoomClosedAsync(n.DomainEvent.RoomId, n.DomainEvent.Reason, ct);
}

public sealed class MatchStartedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<MatchStarted>>
{
    public Task Handle(DomainEventNotification<MatchStarted> n, CancellationToken ct) =>
        notifier.MatchStartedAsync(n.DomainEvent.RoomId, n.DomainEvent.MatchId, n.DomainEvent.CaseCount, ct);
}

/// <summary>The only handler that needs a repository: RoundOpened only carries the CaseFileId, so the case's Autos/Hint text is resolved here before broadcasting.</summary>
public sealed class RoundOpenedHandler(IRoomNotifier notifier, ICaseBankRepository caseBank) : INotificationHandler<DomainEventNotification<RoundOpened>>
{
    public async Task Handle(DomainEventNotification<RoundOpened> n, CancellationToken ct)
    {
        var e = n.DomainEvent;
        var caseFile = await caseBank.GetByIdAsync(e.CaseFileId, ct);
        await notifier.CaseStartedAsync(
            e.RoomId, e.RoundId, e.OrderIndex + 1, e.CaseCount,
            caseFile?.Autos ?? string.Empty, caseFile?.Hint ?? string.Empty, e.DeadlineUtc, ct);
    }
}

public sealed class DefenseSubmittedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<DefenseSubmitted>>
{
    public Task Handle(DomainEventNotification<DefenseSubmitted> n, CancellationToken ct) =>
        notifier.DefenseFiledAsync(n.DomainEvent.RoomId, n.DomainEvent.PlayerId, n.DomainEvent.FiledCount, n.DomainEvent.TotalPlayers, ct);
}

public sealed class RoundClosedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<RoundClosed>>
{
    public Task Handle(DomainEventNotification<RoundClosed> n, CancellationToken ct) =>
        notifier.DeliberationReadyAsync(n.DomainEvent.RoomId, n.DomainEvent.Results, n.DomainEvent.Standings, ct);
}

public sealed class MatchEndedHandler(IRoomNotifier notifier) : INotificationHandler<DomainEventNotification<MatchEnded>>
{
    public Task Handle(DomainEventNotification<MatchEnded> n, CancellationToken ct) =>
        notifier.MatchEndedAsync(n.DomainEvent.RoomId, n.DomainEvent.FinalStandings, n.DomainEvent.WinnerPlayerId, ct);
}
