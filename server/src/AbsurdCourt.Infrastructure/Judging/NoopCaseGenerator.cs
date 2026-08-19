using AbsurdCourt.Application.Abstractions;

namespace AbsurdCourt.Infrastructure.Judging;

/// <summary>Keeps tests and non-Gemini environments on the curated fallback catalogue.</summary>
public sealed class NoopCaseGenerator : ICaseGenerator
{
    public Task<IReadOnlyList<GeneratedCase>> GenerateAsync(int count, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<GeneratedCase>>([]);
}
