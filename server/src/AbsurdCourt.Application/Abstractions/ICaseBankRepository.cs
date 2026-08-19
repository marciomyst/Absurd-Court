using AbsurdCourt.Domain.Matches;

namespace AbsurdCourt.Application.Abstractions;

public interface ICaseBankRepository
{
    Task<CaseFile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetRandomCaseIdsAsync(int count, CancellationToken ct = default);
}
