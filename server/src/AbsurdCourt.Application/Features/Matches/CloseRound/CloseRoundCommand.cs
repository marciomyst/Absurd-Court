using MediatR;

namespace AbsurdCourt.Application.Features.Matches.CloseRound;

/// <summary>
/// Fired from two independent paths — the last player submitting, and the deadline
/// sweeper — which is exactly why the handler leans on Match.TryCloseRound's idempotency
/// gate plus IUnitOfWork's concurrency check instead of trusting a single caller.
/// </summary>
public sealed record CloseRoundCommand(Guid RoomId) : IRequest;
