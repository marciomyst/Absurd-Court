using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Matches;

namespace AbsurdCourt.Application.Tests.Fakes;

public sealed class FakeJudgeService : IJudgeService
{
    public int CallCount { get; private set; }
    public Func<IReadOnlyList<DefenseSubmission>, RoundJudgment>? JudgeOverride { get; set; }

    public Task<RoundJudgment> JudgeRoundAsync(string autos, string hint, IReadOnlyList<DefenseSubmission> submissions, CancellationToken ct = default)
    {
        CallCount++;
        if (JudgeOverride is not null) return Task.FromResult(JudgeOverride(submissions));

        var verdicts = new Dictionary<Guid, Verdict>();
        for (var i = 0; i < submissions.Count; i++)
        {
            verdicts[submissions[i].PlayerId] = i == 0
                ? Verdict.Winner("Parecer padrão de teste.")
                : Verdict.Create("Consolação de teste.", 200);
        }

        return Task.FromResult(new RoundJudgment(verdicts));
    }
}
