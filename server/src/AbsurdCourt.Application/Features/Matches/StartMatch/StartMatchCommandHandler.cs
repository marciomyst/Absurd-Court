using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using AbsurdCourt.Domain.Matches;
using MediatR;

namespace AbsurdCourt.Application.Features.Matches.StartMatch;

public sealed class StartMatchCommandHandler(
    IRoomRepository rooms, IMatchRepository matches, ICaseBankRepository caseBank, IRoomCasePreparation casePreparation, IUnitOfWork uow)
    : IRequestHandler<StartMatchCommand>
{
    public async Task Handle(StartMatchCommand request, CancellationToken ct)
    {
        var room = await rooms.GetByIdAsync(request.RoomId, ct) ?? throw new RoomNotFoundException(request.RoomId.ToString());
        if (request.ConnectionId is not null)
            room.EnsureCurrentConnection(request.RequestedByPlayerId, request.ConnectionId);
        if (room.HostPlayerId != request.RequestedByPlayerId)
            throw new NotHostException(request.RoomId, request.RequestedByPlayerId);

        room.BeginMatch();

        const int initialCaseCount = 3;
        var caseIds = casePreparation.TakeInitial(room.Id, initialCaseCount).ToList();
        var fallbackCaseIds = await caseBank.GetRandomCaseIdsAsync(room.Settings.CaseCount, ct);
        foreach (var fallbackCaseId in fallbackCaseIds)
        {
            if (caseIds.Count == room.Settings.CaseCount) break;
            if (!caseIds.Contains(fallbackCaseId)) caseIds.Add(fallbackCaseId);
        }
        var match = Match.Start(
            room.Id,
            room.Players.Select(p => p.Id).ToList(),
            caseIds,
            room.Settings.RoundDurationSeconds,
            DateTime.UtcNow);
        matches.Add(match);

        await uow.SaveChangesAsync(ct);
        if (room.Settings.CaseCount > initialCaseCount)
            casePreparation.PrepareRemaining(room.Id, room.Settings.CaseCount - initialCaseCount, initialCaseCount);
    }
}
