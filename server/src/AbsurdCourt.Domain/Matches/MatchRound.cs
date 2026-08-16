using AbsurdCourt.Domain.Common;

namespace AbsurdCourt.Domain.Matches;

public enum RoundStatus { Open, Judging, Revealed }

public sealed class MatchRound : Entity
{
    private readonly List<Defense> _defenses = new();

    public Guid CaseFileId { get; private set; }
    public int OrderIndex { get; private set; }
    public DateTime DeadlineUtc { get; private set; }
    public RoundStatus Status { get; private set; }

    public IReadOnlyList<Defense> Defenses => _defenses.AsReadOnly();

    private MatchRound() { }

    internal MatchRound(Guid id, Guid caseFileId, int orderIndex, DateTime deadlineUtc) : base(id)
    {
        CaseFileId = caseFileId;
        OrderIndex = orderIndex;
        DeadlineUtc = deadlineUtc;
        Status = RoundStatus.Open;
    }

    internal void SubmitDefense(Guid playerId, string text, DateTime submittedAtUtc)
    {
        if (Status != RoundStatus.Open) throw new RoundNotOpenException(Id);
        if (_defenses.Any(d => d.PlayerId == playerId)) throw new DefenseAlreadySubmittedException(Id, playerId);

        _defenses.Add(Defense.Submit(playerId, DefenseText.Create(text), submittedAtUtc));
    }

    /// <summary>
    /// Open→Judging transition gate. Returns false if the round was already closed by
    /// another caller — the idempotency guard that lets "last player submitted" and
    /// "deadline swept" race safely without double-judging.
    /// </summary>
    internal bool TryBeginJudging()
    {
        if (Status != RoundStatus.Open) return false;
        Status = RoundStatus.Judging;
        return true;
    }

    /// <summary>
    /// verdictsByPlayer only needs an entry per player who actually submitted a defense
    /// (i.e. what the caller ran through the LLM judge) — a player with no submission gets
    /// "revelia" applied here, since that's a domain rule, not something every caller
    /// should have to know to supply.
    /// </summary>
    internal void Reveal(IReadOnlyCollection<Guid> allPlayerIds, IReadOnlyDictionary<Guid, Verdict> verdictsByPlayer, DateTime nowUtc)
    {
        if (Status != RoundStatus.Judging) throw new RoundNotJudgingException(Id);

        foreach (var playerId in allPlayerIds)
        {
            var defense = _defenses.FirstOrDefault(d => d.PlayerId == playerId);
            if (defense is null)
            {
                defense = Defense.Default(playerId, nowUtc);
                _defenses.Add(defense);
                defense.ApplyVerdict(Verdict.Revelia());
                continue;
            }

            if (!verdictsByPlayer.TryGetValue(playerId, out var verdict))
                throw new ArgumentException($"Falta veredito para o jogador {playerId}, que protocolou defesa.", nameof(verdictsByPlayer));

            defense.ApplyVerdict(verdict);
        }

        Status = RoundStatus.Revealed;
    }

    public bool AllPlayersFiled(IReadOnlyCollection<Guid> playerIds) =>
        playerIds.Count > 0 && playerIds.All(id => _defenses.Any(d => d.PlayerId == id));
}
