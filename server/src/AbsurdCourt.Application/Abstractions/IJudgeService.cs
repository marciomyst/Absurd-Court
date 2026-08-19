using AbsurdCourt.Domain.Matches;

namespace AbsurdCourt.Application.Abstractions;

public sealed record DefenseSubmission(Guid PlayerId, string Text);

/// <summary>One player's Verdict.Winner(...) among the round's submissions — exactly one ruling per round, like the prototype's flat +1000 for the case's winner.</summary>
public sealed record RoundJudgment(IReadOnlyDictionary<Guid, Verdict> VerdictsByPlayer);

public interface IJudgeService
{
    /// <summary>
    /// Judges an entire round at once (not one call per defense): the judge compares all
    /// submissions for the case and rules in favor of exactly one, awarding it full marks,
    /// while everyone else gets an individually-reasoned consolation score. Implementations
    /// are expected to fall back to a neutral judgment for everyone rather than throw if the
    /// underlying LLM call fails — a single API hiccup shouldn't stall the room.
    /// </summary>
    Task<RoundJudgment> JudgeRoundAsync(string autos, string hint, IReadOnlyList<DefenseSubmission> submissions, CancellationToken ct = default);
}
