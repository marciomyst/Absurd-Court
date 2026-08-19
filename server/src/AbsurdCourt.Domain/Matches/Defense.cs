using AbsurdCourt.Domain.Common;

namespace AbsurdCourt.Domain.Matches;

public sealed class Defense : Entity
{
    public Guid PlayerId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public bool WasSubmitted { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public Verdict? Verdict { get; private set; }

    private Defense() { }

    private Defense(Guid id, Guid playerId, string text, bool wasSubmitted, DateTime submittedAtUtc) : base(id)
    {
        PlayerId = playerId;
        Text = text;
        WasSubmitted = wasSubmitted;
        SubmittedAtUtc = submittedAtUtc;
    }

    internal static Defense Submit(Guid playerId, DefenseText text, DateTime submittedAtUtc) =>
        new(Guid.NewGuid(), playerId, text.Value, wasSubmitted: true, submittedAtUtc);

    /// <summary>"Revelia" placeholder for a player who never submitted before the round closed.</summary>
    internal static Defense Default(Guid playerId, DateTime nowUtc) =>
        new(Guid.NewGuid(), playerId, string.Empty, wasSubmitted: false, nowUtc);

    internal void ApplyVerdict(Verdict verdict) => Verdict = verdict;
}
