using AbsurdCourt.Application.Contracts;
using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.JoinRoom;

public sealed record JoinRoomCommand(string RoomCode, string PlayerName, string ConnectionId) : IRequest<JoinRoomResult>;

public sealed record JoinRoomResult(RoomSnapshotDto Room, Guid YourPlayerId, [property: System.Text.Json.Serialization.JsonIgnore] Guid ReconnectToken);
