using AbsurdCourt.Domain.Common;
using AbsurdCourt.Domain.Events;

namespace AbsurdCourt.Domain.Matches;

public enum MatchStatus { InProgress, Completed }

public sealed class Match : AggregateRoot
{
    private readonly List<Guid> _caseFileSequence = new();
    private readonly List<MatchRound> _rounds = new();
    private readonly Dictionary<Guid, int> _scores = new();

    public Guid RoomId { get; private set; }
    public MatchStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }
    public int RoundDurationSeconds { get; private set; }

    public int CaseCount => _caseFileSequence.Count;
    public IReadOnlyList<MatchRound> Rounds => _rounds.AsReadOnly();
    public IReadOnlyDictionary<Guid, int> Scores => _scores;

    /// <summary>
    /// Ordered by OrderIndex explicitly rather than trusting _rounds' materialization
    /// order: EF Core doesn't guarantee a collection navigation loads back in insertion
    /// order without an explicit ORDER BY, so relying on "last in the list" silently
    /// picked an already-Revealed round after a reload — TryCloseRound then quietly
    /// no-opped forever on the real current round.
    /// </summary>
    public MatchRound? CurrentRound => _rounds.Count == 0 ? null : _rounds.OrderBy(r => r.OrderIndex).Last();

    public bool HasMoreRounds => _rounds.Count < CaseCount;

    private Match() { }

    private Match(Guid id, Guid roomId, IReadOnlyList<Guid> caseFileSequence, IReadOnlyCollection<Guid> playerIds, int roundDurationSeconds, DateTime nowUtc) : base(id)
    {
        RoomId = roomId;
        Status = MatchStatus.InProgress;
        StartedAtUtc = nowUtc;
        RoundDurationSeconds = roundDurationSeconds;
        _caseFileSequence.AddRange(caseFileSequence);
        foreach (var playerId in playerIds) _scores[playerId] = 0;
    }

    /// <summary>
    /// caseFileSequence is the full, pre-selected order of cases for the whole match (chosen by
    /// the Application layer via ICaseBankRepository, sized to the room's configured case count) —
    /// Match remembers it so NextRound doesn't need the caller to re-supply which case comes next.
    /// roundDurationSeconds comes from the room's settings (30/45/60s), chosen by the host pre-match.
    /// </summary>
    public static Match Start(Guid roomId, IReadOnlyCollection<Guid> playerIds, IReadOnlyList<Guid> caseFileSequence, int roundDurationSeconds, DateTime nowUtc)
    {
        if (playerIds.Count < 2) throw new NotEnoughPlayersException(roomId);
        if (caseFileSequence.Count == 0) throw new NoCasesAvailableException(roomId);

        var match = new Match(Guid.NewGuid(), roomId, caseFileSequence, playerIds, roundDurationSeconds, nowUtc);
        match.OpenRound(nowUtc);
        match.Raise(new MatchStarted(match.Id, roomId, match.CaseCount));
        return match;
    }

    private void OpenRound(DateTime nowUtc)
    {
        var caseFileId = _caseFileSequence[_rounds.Count];
        var round = new MatchRound(Guid.NewGuid(), caseFileId, _rounds.Count, nowUtc.AddSeconds(RoundDurationSeconds));
        _rounds.Add(round);
        Raise(new RoundOpened(Id, RoomId, round.Id, caseFileId, round.OrderIndex, CaseCount, round.DeadlineUtc));
    }

    public void SubmitDefense(Guid playerId, string text, DateTime nowUtc)
    {
        if (!_scores.ContainsKey(playerId)) throw new PlayerNotInMatchException(Id, playerId);

        var round = CurrentRound ?? throw new RoundNotOpenException(Id);
        round.SubmitDefense(playerId, text, nowUtc);
        Raise(new DefenseSubmitted(Id, RoomId, round.Id, playerId, round.Defenses.Count, _scores.Count));
    }

    public bool ConnectedPlayersAllFiled(IReadOnlyCollection<Guid> connectedPlayerIds) =>
        CurrentRound?.AllPlayersFiled(connectedPlayerIds) ?? false;

    /// <summary>
    /// Idempotency gate for round closing: false means another caller (the "last player
    /// submitted" path or the deadline sweeper) already won the race to close this round.
    /// </summary>
    public bool TryCloseRound(out MatchRound round)
    {
        round = CurrentRound ?? throw new RoundNotOpenException(Id);
        return round.TryBeginJudging();
    }

    public void RevealRound(IReadOnlyDictionary<Guid, Verdict> verdictsByPlayer, DateTime nowUtc)
    {
        var round = CurrentRound ?? throw new RoundNotOpenException(Id);
        round.Reveal(_scores.Keys.ToList(), verdictsByPlayer, nowUtc);

        foreach (var defense in round.Defenses)
        {
            if (defense.Verdict is not null)
                _scores[defense.PlayerId] += defense.Verdict.Points;
        }

        Raise(new RoundClosed(
            Id, RoomId, round.Id, round.OrderIndex, CaseCount,
            round.Defenses
                .Select(d => new RoundResult(d.PlayerId, d.Text, d.WasSubmitted, d.Verdict!.ParecerText, d.Verdict.Points))
                .ToList(),
            new Dictionary<Guid, int>(_scores)));
    }

    public void AdvanceToNextRound(DateTime nowUtc)
    {
        if (CurrentRound?.Status != RoundStatus.Revealed) throw new RoundNotRevealedException(Id);
        if (!HasMoreRounds) throw new NoMoreRoundsException(Id);

        OpenRound(nowUtc);
    }

    public void Complete(DateTime nowUtc)
    {
        if (CurrentRound?.Status != RoundStatus.Revealed) throw new RoundNotRevealedException(Id);
        if (HasMoreRounds) throw new MoreRoundsRemainException(Id);

        Status = MatchStatus.Completed;
        EndedAtUtc = nowUtc;
        var winnerId = _scores.OrderByDescending(kv => kv.Value).First().Key;
        Raise(new MatchEnded(Id, RoomId, new Dictionary<Guid, int>(_scores), winnerId));
    }
}
