using MediatR;

namespace AbsurdCourt.Application.Features.Matches.NextRound;

public sealed record NextRoundCommand(Guid RoomId, Guid RequestedByPlayerId, string? ConnectionId = null) : IRequest;
