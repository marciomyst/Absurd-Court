using AbsurdCourt.Application.Abstractions;

namespace AbsurdCourt.Application.Tests.Fakes;

/// <summary>
/// The fakes keep the same in-memory aggregate instance across "loads", so
/// Match.TryCloseRound's own idempotency check is what the CloseRound tests exercise —
/// no need to simulate EF's concurrency token here.
/// </summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
