using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using AbsurdCourt.Domain.Matches;
using MediatR;

namespace AbsurdCourt.Application.Features.Matches.StartMatch;

public sealed class StartMatchCommandHandler(
    IRoomRepository rooms, IMatchRepository matches, ICaseBankRepository caseBank, IUnitOfWork uow)
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

        var caseIds = await caseBank.GetRandomCaseIdsAsync(room.Settings.CaseCount, ct);
        var match = Match.Start(
            room.Id,
            room.Players.Select(p => p.Id).ToList(),
            caseIds,
            room.Settings.RoundDurationSeconds,
            DateTime.UtcNow);
        matches.Add(match);

        await uow.SaveChangesAsync(ct);
    }
}
