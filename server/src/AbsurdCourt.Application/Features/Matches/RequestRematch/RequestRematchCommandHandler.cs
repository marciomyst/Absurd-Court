using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using AbsurdCourt.Application.Features.Matches.StartMatch;
using MediatR;

namespace AbsurdCourt.Application.Features.Matches.RequestRematch;

public sealed class RequestRematchCommandHandler(IRoomRepository rooms, IUnitOfWork uow, ISender sender)
    : IRequestHandler<RequestRematchCommand>
{
    public async Task Handle(RequestRematchCommand request, CancellationToken ct)
    {
        var room = await rooms.GetByIdAsync(request.RoomId, ct) ?? throw new RoomNotFoundException(request.RoomId.ToString());
        if (request.ConnectionId is not null)
            room.EnsureCurrentConnection(request.PlayerId, request.ConnectionId);

        var allReady = room.SetRematchReady(request.PlayerId, request.WantsRematch);
        await uow.SaveChangesAsync(ct);

        // All connected players opted in: kick off the next match ourselves, as if the
        // host had called StartMatch — reuses the same validation/case-selection logic.
        if (allReady)
            await sender.Send(new StartMatchCommand(request.RoomId, room.HostPlayerId), ct);
    }
}
