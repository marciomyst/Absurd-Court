using System.Text.RegularExpressions;
using AbsurdCourt.Application.Abstractions;

namespace AbsurdCourt.Infrastructure.Judging;

/// <summary>Juiz determinístico para ambientes locais e E2E, sem chamadas externas.</summary>
public sealed partial class MockAiProvider : IAiProvider
{
    public Task<AiJudgeResponse> JudgeAsync(AiJudgeRequest request, CancellationToken ct = default)
    {
        var labels = DefendantLabelRegex()
            .Matches(request.UserPrompt)
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (labels.Count == 0)
        {
            labels.Add("A");
        }

        var rulings = labels
            .Select((label, index) => new AiRuling(
                label,
                IsWinner: index == 0,
                Opinion: index == 0
                    ? $"O réu {label} apresentou a justificativa mais meticulosamente absurda dos autos."
                    : $"O réu {label} teve sua manifestação registrada com a solenidade cabível.",
                Points: index == 0 ? 1000 : 300 + Math.Min(index * 50, 300)))
            .ToList();

        return Task.FromResult(new AiJudgeResponse(rulings));
    }

    [GeneratedRegex("(?m)^\\s*R[\\p{L}]*\\s+([A-Z])\\s*:")]
    private static partial Regex DefendantLabelRegex();
}
