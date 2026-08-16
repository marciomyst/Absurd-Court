using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Matches;

namespace AbsurdCourt.Application.Tests.Fakes;

public sealed class FakeCaseBankRepository(int seedCount = 20) : ICaseBankRepository
{
    private readonly List<CaseFile> _cases = Enumerable.Range(0, seedCount)
        .Select(i => new CaseFile(Guid.NewGuid(), $"Autos de teste {i}", $"Dica {i}"))
        .ToList();

    public Task<CaseFile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_cases.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Guid>> GetRandomCaseIdsAsync(int count, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Guid>>(_cases.Take(count).Select(c => c.Id).ToList());
}
