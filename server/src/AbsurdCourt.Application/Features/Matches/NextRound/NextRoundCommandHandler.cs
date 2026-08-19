using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using MediatR;

namespace AbsurdCourt.Application.Features.Matches.NextRound;

public sealed class NextRoundCommandHandler(IRoomRepository rooms, IMatchRepository matches, IUnitOfWork uow)
    : IRequestHandler<NextRoundCommand>
{
    public async Task Handle(NextRoundCommand request, CancellationToken ct)
    {
        var room = await rooms.GetByIdAsync(request.RoomId, ct) ?? throw new RoomNotFoundException(request.RoomId.ToString());
        if (request.ConnectionId is not null)
            room.EnsureCurrentConnection(request.RequestedByPlayerId, request.ConnectionId);
        if (room.HostPlayerId != request.RequestedByPlayerId)
            throw new NotHostException(request.RoomId, request.RequestedByPlayerId);

        var match = await matches.GetActiveByRoomIdAsync(request.RoomId, ct)
            ?? throw new MatchNotFoundException(request.RoomId);

        if (match.HasMoreRounds)
        {
            match.AdvanceToNextRound(DateTime.UtcNow);
        }
        else
        {
            match.Complete(DateTime.UtcNow);
            room.EndMatch(DateTime.UtcNow);
        }

        await uow.SaveChangesAsync(ct);
    }
}
