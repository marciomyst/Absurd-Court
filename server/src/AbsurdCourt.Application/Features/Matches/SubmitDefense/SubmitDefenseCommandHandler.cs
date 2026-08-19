using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using AbsurdCourt.Application.Features.Matches.CloseRound;
using MediatR;

namespace AbsurdCourt.Application.Features.Matches.SubmitDefense;

public sealed class SubmitDefenseCommandHandler(
    IRoomRepository rooms, IMatchRepository matches, IUnitOfWork uow, ISender sender)
    : IRequestHandler<SubmitDefenseCommand>
{
    public async Task Handle(SubmitDefenseCommand request, CancellationToken ct)
    {
        var match = await matches.GetActiveByRoomIdAsync(request.RoomId, ct)
            ?? throw new MatchNotFoundException(request.RoomId);

        var room = await rooms.GetByIdAsync(request.RoomId, ct) ?? throw new RoomNotFoundException(request.RoomId.ToString());
        if (request.ConnectionId is not null)
            room.EnsureCurrentConnection(request.PlayerId, request.ConnectionId);
        match.SubmitDefense(request.PlayerId, request.Text, DateTime.UtcNow);
        await uow.SaveChangesAsync(ct);

        // If everyone still connected has now filed, close the round immediately instead of
        // waiting for the deadline sweeper — same CloseRoundCommand either path ends up at.
        if (match.ConnectedPlayersAllFiled(room.ConnectedPlayerIds))
            await sender.Send(new CloseRoundCommand(request.RoomId), ct);
    }
}
