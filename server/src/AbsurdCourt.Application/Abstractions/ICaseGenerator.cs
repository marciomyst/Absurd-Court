namespace AbsurdCourt.Application.Abstractions;

public sealed record GeneratedCase(string Hint, string Autos);

public interface ICaseGenerator
{
    Task<IReadOnlyList<GeneratedCase>> GenerateAsync(int count, CancellationToken ct = default);
}
