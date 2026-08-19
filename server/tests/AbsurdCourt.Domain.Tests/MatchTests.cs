using AbsurdCourt.Domain.Events;
using AbsurdCourt.Domain.Matches;

namespace AbsurdCourt.Domain.Tests;

public class MatchTests
{
    private static readonly DateTime Now = new(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid RoomId = Guid.NewGuid();
    private static readonly Guid Alice = Guid.NewGuid();
    private static readonly Guid Bob = Guid.NewGuid();
    private static readonly Guid Carol = Guid.NewGuid();

    private static Match StartMatch(int caseCount = 3, params Guid[] players)
    {
        var cases = Enumerable.Range(0, caseCount).Select(_ => Guid.NewGuid()).ToList();
        var playerIds = players.Length > 0 ? players : new[] { Alice, Bob };
        return Match.Start(RoomId, playerIds, cases, 60, Now);
    }

    [Fact]
    public void Start_with_fewer_than_two_players_throws() =>
        Assert.Throws<AbsurdCourt.Domain.Common.NotEnoughPlayersException>(
            () => Match.Start(RoomId, [Alice], [Guid.NewGuid()], 60, Now));

    [Fact]
    public void Start_with_no_cases_throws() =>
        Assert.Throws<NoCasesAvailableException>(
            () => Match.Start(RoomId, [Alice, Bob], [], 60, Now));

    [Fact]
    public void Start_opens_first_round()
    {
        var match = StartMatch();

        Assert.NotNull(match.CurrentRound);
        Assert.Equal(RoundStatus.Open, match.CurrentRound!.Status);
        Assert.Equal(0, match.CurrentRound.OrderIndex);
    }

    [Fact]
    public void SubmitDefense_twice_by_same_player_throws()
    {
        var match = StartMatch();
        match.SubmitDefense(Alice, "Não fui eu, foi o flamingo.", Now);

        Assert.Throws<DefenseAlreadySubmittedException>(
            () => match.SubmitDefense(Alice, "Segunda tentativa.", Now));
    }

    [Fact]
    public void SubmitDefense_by_unknown_player_throws()
    {
        var match = StartMatch();

        Assert.Throws<PlayerNotInMatchException>(
            () => match.SubmitDefense(Guid.NewGuid(), "Quem sou eu?", Now));
    }

    [Fact]
    public void SubmitDefense_over_length_limit_throws()
    {
        var match = StartMatch();
        var tooLong = new string('a', DefenseText.MaxLength + 1);

        Assert.Throws<ArgumentException>(() => match.SubmitDefense(Alice, tooLong, Now));
    }

    [Fact]
    public void TryCloseRound_second_call_returns_false()
    {
        var match = StartMatch();
        match.SubmitDefense(Alice, "Defesa da Alice", Now);

        var first = match.TryCloseRound(out _);
        var second = match.TryCloseRound(out _);

        Assert.True(first);
        Assert.False(second, "a segunda chamada não deve reabrir/refechar a rodada — é a garantia de idempotência");
    }

    [Fact]
    public void RevealRound_applies_revelia_to_players_who_never_submitted()
    {
        var match = StartMatch(players: [Alice, Bob]);
        match.SubmitDefense(Alice, "Defesa da Alice", Now);
        match.TryCloseRound(out _);

        match.RevealRound(new Dictionary<Guid, Verdict> { [Alice] = Verdict.Create("Parecer favorável.", 800) }, Now);

        Assert.Equal(800, match.Scores[Alice]);
        Assert.Equal(0, match.Scores[Bob]);
        var bobDefense = match.CurrentRound!.Defenses.Single(d => d.PlayerId == Bob);
        Assert.False(bobDefense.WasSubmitted);
        Assert.Equal(0, bobDefense.Verdict!.Points);
    }

    [Fact]
    public void RevealRound_before_judging_throws()
    {
        var match = StartMatch();

        Assert.Throws<RoundNotJudgingException>(() => match.RevealRound(new Dictionary<Guid, Verdict>(), Now));
    }

    [Fact]
    public void AdvanceToNextRound_before_reveal_throws()
    {
        var match = StartMatch();

        Assert.Throws<RoundNotRevealedException>(() => match.AdvanceToNextRound(Now));
    }

    [Fact]
    public void AdvanceToNextRound_moves_to_next_case_in_sequence()
    {
        var match = StartMatch(caseCount: 2, Alice, Bob);
        CloseAndRevealCurrentRound(match);

        match.AdvanceToNextRound(Now);

        Assert.Equal(2, match.Rounds.Count);
        Assert.Equal(1, match.CurrentRound!.OrderIndex);
        Assert.Equal(RoundStatus.Open, match.CurrentRound.Status);
    }

    [Fact]
    public void ReplaceFutureCaseFiles_keeps_current_case_and_updates_the_next_one()
    {
        var firstCase = Guid.NewGuid();
        var fallbackCase = Guid.NewGuid();
        var generatedCase = Guid.NewGuid();
        var match = Match.Start(RoomId, [Alice, Bob], [firstCase, fallbackCase], 60, Now);

        match.ReplaceFutureCaseFiles(1, [generatedCase]);

        Assert.Equal(firstCase, match.CurrentRound!.CaseFileId);
        CloseAndRevealCurrentRound(match);
        match.AdvanceToNextRound(Now);
        Assert.Equal(generatedCase, match.CurrentRound!.CaseFileId);
    }

    [Fact]
    public void Complete_before_last_round_revealed_throws()
    {
        var match = StartMatch(caseCount: 2, Alice, Bob);

        Assert.Throws<RoundNotRevealedException>(() => match.Complete(Now));
    }

    [Fact]
    public void Complete_while_rounds_remain_throws()
    {
        var match = StartMatch(caseCount: 2, Alice, Bob);
        CloseAndRevealCurrentRound(match);

        Assert.Throws<MoreRoundsRemainException>(() => match.Complete(Now));
    }

    [Fact]
    public void Complete_picks_highest_scorer_as_winner()
    {
        var match = StartMatch(caseCount: 1, Alice, Bob, Carol);
        match.SubmitDefense(Alice, "Defesa fraca", Now);
        match.SubmitDefense(Bob, "Defesa brilhante", Now);
        match.SubmitDefense(Carol, "Defesa mediana", Now);
        match.TryCloseRound(out _);
        match.RevealRound(new Dictionary<Guid, Verdict>
        {
            [Alice] = Verdict.Create("Fraco.", 100),
            [Bob] = Verdict.Create("Brilhante!", 950),
            [Carol] = Verdict.Create("Mediano.", 400),
        }, Now);

        match.Complete(Now);

        Assert.Equal(MatchStatus.Completed, match.Status);
        var ended = match.DomainEvents.OfType<MatchEnded>().Single();
        Assert.Equal(Bob, ended.WinnerPlayerId);
    }

    private static void CloseAndRevealCurrentRound(Match match)
    {
        match.TryCloseRound(out _);
        match.RevealRound(new Dictionary<Guid, Verdict>(), Now);
    }
}
