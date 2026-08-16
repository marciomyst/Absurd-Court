using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using AbsurdCourt.Application.Contracts;
using AbsurdCourt.Domain.Rooms;
using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.JoinRoom;

public sealed class JoinRoomCommandHandler(IRoomRepository rooms, IUnitOfWork uow)
    : IRequestHandler<JoinRoomCommand, JoinRoomResult>
{
    public async Task<JoinRoomResult> Handle(JoinRoomCommand request, CancellationToken ct)
    {
        var code = RoomCode.Create(request.RoomCode);
        var room = await rooms.GetByCodeAsync(code, ct) ?? throw new RoomNotFoundException(request.RoomCode);

        var player = room.AddPlayer(request.PlayerName, request.ConnectionId, DateTime.UtcNow);
        await uow.SaveChangesAsync(ct);

        return new JoinRoomResult(room.ToSnapshot(), player.Id, player.ReconnectToken);
    }
}
