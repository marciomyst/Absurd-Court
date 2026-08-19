using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.LeaveRoom;

public sealed class LeaveRoomCommandHandler(IRoomRepository rooms, IUnitOfWork uow) : IRequestHandler<LeaveRoomCommand>
{
    public async Task Handle(LeaveRoomCommand request, CancellationToken ct)
    {
        var room = await rooms.GetByIdAsync(request.RoomId, ct)
            ?? throw new RoomNotFoundException(request.RoomId.ToString());

        if (!room.IsCurrentConnection(request.PlayerId, request.ConnectionId))
            throw new ConnectionNotAuthorizedException(request.PlayerId);

        room.Leave(request.PlayerId);
        await uow.SaveChangesAsync(ct);
    }
}
