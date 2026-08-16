namespace AbsurdCourt.Application.Abstractions;

/// <summary>
/// SaveChangesAsync also dispatches every pending domain event raised by tracked
/// aggregates (published via MediatR after a successful save) and throws
/// ConcurrencyConflictException if a tracked aggregate was concurrently modified —
/// that's what makes CloseRound's Open→Judging transition safe to race between the
/// "last player submitted" path and the deadline sweeper.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
