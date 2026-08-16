using AbsurdCourt.Application.Contracts;
using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.CreateRoom;

public sealed record CreateRoomCommand(string HostName, string ConnectionId) : IRequest<CreateRoomResult>;

public sealed record CreateRoomResult(RoomSnapshotDto Room, Guid YourPlayerId, [property: System.Text.Json.Serialization.JsonIgnore] Guid ReconnectToken);
