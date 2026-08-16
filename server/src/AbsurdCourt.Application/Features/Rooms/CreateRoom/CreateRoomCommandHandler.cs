using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Contracts;
using AbsurdCourt.Domain.Rooms;
using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.CreateRoom;

public sealed class CreateRoomCommandHandler(IRoomRepository rooms, IUnitOfWork uow)
    : IRequestHandler<CreateRoomCommand, CreateRoomResult>
{
    public async Task<CreateRoomResult> Handle(CreateRoomCommand request, CancellationToken ct)
    {
        RoomCode code;
        do { code = RoomCode.Generate(); }
        while (await rooms.ExistsByCodeAsync(code, ct));

        var (room, host) = Room.Create(code, request.HostName, request.ConnectionId, DateTime.UtcNow);
        rooms.Add(room);
        await uow.SaveChangesAsync(ct);

        return new CreateRoomResult(room.ToSnapshot(), host.Id, host.ReconnectToken);
    }
}
