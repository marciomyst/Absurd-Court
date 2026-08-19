using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.LeaveRoom;

public sealed record LeaveRoomCommand(Guid RoomId, Guid PlayerId, string ConnectionId) : IRequest;
