using MediatR;

namespace AbsurdCourt.Application.Features.Matches.RequestRematch;

public sealed record RequestRematchCommand(Guid RoomId, Guid PlayerId, bool WantsRematch, string? ConnectionId = null) : IRequest;
