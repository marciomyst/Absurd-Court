using AbsurdCourt.Application.Abstractions;
using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.Disconnect;

public sealed class DisconnectCommandHandler(IRoomRepository rooms, IUnitOfWork uow) : IRequestHandler<DisconnectCommand>
{
    public async Task Handle(DisconnectCommand request, CancellationToken ct)
    {
        var room = await rooms.GetByIdAsync(request.RoomId, ct);
        if (room is null) return; // room already gone — nothing to mark

        if (request.ConnectionId is not null && !room.IsCurrentConnection(request.PlayerId, request.ConnectionId)) return;

        room.Disconnect(request.PlayerId);
        await uow.SaveChangesAsync(ct);
    }
}
