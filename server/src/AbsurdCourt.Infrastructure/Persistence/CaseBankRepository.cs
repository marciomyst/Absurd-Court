using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Matches;
using Microsoft.EntityFrameworkCore;

namespace AbsurdCourt.Infrastructure.Persistence;

public sealed class CaseBankRepository(CourtDbContext db) : ICaseBankRepository
{
    public Task<CaseFile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.CaseFiles.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Guid>> GetRandomCaseIdsAsync(int count, CancellationToken ct = default)
    {
        // The bank is small (a couple dozen seeded cases) — shuffling in memory is simpler
        // and more portable than relying on a provider-specific SQL random() function.
        var allIds = await db.CaseFiles.Select(c => c.Id).ToListAsync(ct);
        return allIds.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }
}
