using MediatR;

namespace AbsurdCourt.Application.Features.Matches.SubmitDefense;

public sealed record SubmitDefenseCommand(Guid RoomId, Guid PlayerId, string Text, string? ConnectionId = null) : IRequest;
