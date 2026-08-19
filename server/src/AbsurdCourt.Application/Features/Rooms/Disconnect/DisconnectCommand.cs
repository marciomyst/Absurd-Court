using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.Disconnect;

public sealed record DisconnectCommand(Guid RoomId, Guid PlayerId, string? ConnectionId = null) : IRequest;
