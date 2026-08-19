using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Application.Common;
using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.UpdateSettings;

public sealed class UpdateSettingsCommandHandler(IRoomRepository rooms, IUnitOfWork uow)
    : IRequestHandler<UpdateSettingsCommand>
{
    public async Task Handle(UpdateSettingsCommand request, CancellationToken ct)
    {
        var room = await rooms.GetByIdAsync(request.RoomId, ct) ?? throw new RoomNotFoundException(request.RoomId.ToString());
        if (request.ConnectionId is not null)
            room.EnsureCurrentConnection(request.RequestedByPlayerId, request.ConnectionId);
        if (room.HostPlayerId != request.RequestedByPlayerId)
            throw new NotHostException(request.RoomId, request.RequestedByPlayerId);

        room.UpdateSettings(request.CaseCount, request.RoundDurationSeconds);
        await uow.SaveChangesAsync(ct);
    }
}
