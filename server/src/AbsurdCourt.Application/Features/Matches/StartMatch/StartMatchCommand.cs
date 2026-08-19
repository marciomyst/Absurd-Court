using MediatR;

namespace AbsurdCourt.Application.Features.Matches.StartMatch;

public sealed record StartMatchCommand(Guid RoomId, Guid RequestedByPlayerId, string? ConnectionId = null) : IRequest;
