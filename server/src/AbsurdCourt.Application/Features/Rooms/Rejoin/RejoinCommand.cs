using AbsurdCourt.Application.Contracts;
using MediatR;

using System.Text.Json.Serialization;

namespace AbsurdCourt.Application.Features.Rooms.Rejoin;

public sealed record RejoinCommand(string RoomCode, Guid ReconnectToken, string ConnectionId) : IRequest<RejoinResult>;

public sealed record RejoinResult(RoomSnapshotDto Room, Guid YourPlayerId, [property: JsonIgnore] Guid ReconnectToken, MatchSnapshotDto? Match)
{
    [JsonIgnore]
    public string? PreviousConnectionId { get; init; }
}
