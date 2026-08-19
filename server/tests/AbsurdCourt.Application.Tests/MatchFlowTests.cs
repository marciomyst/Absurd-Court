using AbsurdCourt.Application.Common;
using AbsurdCourt.Application.Features.Matches.CloseRound;
using AbsurdCourt.Application.Features.Matches.NextRound;
using AbsurdCourt.Application.Features.Matches.RequestRematch;
using AbsurdCourt.Application.Features.Matches.StartMatch;
using AbsurdCourt.Application.Features.Matches.SubmitDefense;
using AbsurdCourt.Application.Features.Rooms.CreateRoom;
using AbsurdCourt.Application.Features.Rooms.JoinRoom;
using AbsurdCourt.Application.Features.Rooms.Rejoin;
using AbsurdCourt.Application.Tests.Fakes;
using AbsurdCourt.Domain.Matches;
using AbsurdCourt.Domain.Rooms;

namespace AbsurdCourt.Application.Tests;

public class MatchFlowTests
{
    private async Task<(TestHarness Harness, Guid RoomId, Guid HostId, Guid GuestId)> StartTwoPlayerMatchAsync()
    {
        var harness = new TestHarness();
        var created = await harness.Sender.Send(new CreateRoomCommand("Host", "conn-host"));
        var joined = await harness.Sender.Send(new JoinRoomCommand(created.Room.RoomCode, "Guest", "conn-guest"));
        await harness.Sender.Send(new StartMatchCommand(created.Room.RoomId, created.YourPlayerId));

        return (harness, created.Room.RoomId, created.YourPlayerId, joined.YourPlayerId);
    }

    [Fact]
    public async Task SubmitDefense_by_all_players_auto_closes_and_judges_the_round()
    {
        var (harness, roomId, hostId, guestId) = await StartTwoPlayerMatchAsync();

        await harness.Sender.Send(new SubmitDefenseCommand(roomId, hostId, "Defesa do anfitrião"));
        await harness.Sender.Send(new SubmitDefenseCommand(roomId, guestId, "Defesa do convidado"));

        var match = await harness.Matches.GetActiveByRoomIdAsync(roomId);
        Assert.Equal(RoundStatus.Revealed, match!.CurrentRound!.Status);
        Assert.Equal(1, harness.Judge.CallCount);
        Assert.True(match.Scores[hostId] == Verdict.MaxPoints || match.Scores[guestId] == Verdict.MaxPoints);
    }

    [Fact]
    public async Task CloseRound_called_again_after_auto_close_is_a_no_op()
    {
        var (harness, roomId, hostId, guestId) = await StartTwoPlayerMatchAsync();
        await harness.Sender.Send(new SubmitDefenseCommand(roomId, hostId, "Defesa do anfitrião"));
        await harness.Sender.Send(new SubmitDefenseCommand(roomId, guestId, "Defesa do convidado"));

        // Simulates the deadline sweeper firing after the round already auto-closed.
        await harness.Sender.Send(new CloseRoundCommand(roomId));

        Assert.Equal(1, harness.Judge.CallCount);
    }

    [Fact]
    public async Task CloseRound_after_deadline_with_no_defenses_reveals_revelia_without_calling_the_judge()
    {
        var (harness, roomId, hostId, guestId) = await StartTwoPlayerMatchAsync();

        await harness.Sender.Send(new CloseRoundCommand(roomId));

        var match = await harness.Matches.GetActiveByRoomIdAsync(roomId);
        Assert.Equal(RoundStatus.Revealed, match!.CurrentRound!.Status);
        Assert.Equal(0, harness.Judge.CallCount);
        Assert.Equal(0, match.Scores[hostId]);
        Assert.Equal(0, match.Scores[guestId]);
    }

    [Fact]
    public async Task CloseRound_after_deadline_with_one_defense_judges_that_defense_and_marks_the_other_player_in_default()
    {
        var (harness, roomId, hostId, guestId) = await StartTwoPlayerMatchAsync();
        await harness.Sender.Send(new SubmitDefenseCommand(roomId, hostId, "Defesa protocolada"));

        await harness.Sender.Send(new CloseRoundCommand(roomId));

        var match = await harness.Matches.GetActiveByRoomIdAsync(roomId);
        var defenses = match!.CurrentRound!.Defenses;
        Assert.Equal(RoundStatus.Revealed, match.CurrentRound.Status);
        Assert.Equal(1, harness.Judge.CallCount);
        Assert.True(defenses.Single(defense => defense.PlayerId == hostId).WasSubmitted);
        Assert.False(defenses.Single(defense => defense.PlayerId == guestId).WasSubmitted);
        Assert.Equal(0, match.Scores[guestId]);
    }

    [Fact]
    public async Task NextRound_by_non_host_throws()
    {
        var (harness, roomId, _, guestId) = await StartTwoPlayerMatchAsync();

        await Assert.ThrowsAsync<NotHostException>(
            () => harness.Sender.Send(new NextRoundCommand(roomId, guestId)));
    }

    [Fact]
    public async Task Playing_all_rounds_completes_the_match_and_returns_room_to_lobby()
    {
        var (harness, roomId, hostId, guestId) = await StartTwoPlayerMatchAsync();
        var match = await harness.Matches.GetActiveByRoomIdAsync(roomId);
        var caseCount = match!.CaseCount;

        for (var i = 0; i < caseCount; i++)
        {
            await harness.Sender.Send(new SubmitDefenseCommand(roomId, hostId, $"Defesa {i}"));
            await harness.Sender.Send(new SubmitDefenseCommand(roomId, guestId, $"Defesa {i}"));
            await harness.Sender.Send(new NextRoundCommand(roomId, hostId));
        }

        var room = await harness.Rooms.GetByIdAsync(roomId);
        var finishedMatch = await harness.Matches.GetByIdAsync(match.Id);
        Assert.Equal(RoomStatus.Lobby, room!.Status);
        Assert.Equal(MatchStatus.Completed, finishedMatch!.Status);
    }

    [Fact]
    public async Task RequestRematch_once_all_players_ready_starts_a_new_match()
    {
        var (harness, roomId, hostId, guestId) = await StartTwoPlayerMatchAsync();
        var firstMatch = await harness.Matches.GetActiveByRoomIdAsync(roomId);
        var caseCount = firstMatch!.CaseCount;

        for (var i = 0; i < caseCount; i++)
        {
            await harness.Sender.Send(new SubmitDefenseCommand(roomId, hostId, $"Defesa {i}"));
            await harness.Sender.Send(new SubmitDefenseCommand(roomId, guestId, $"Defesa {i}"));
            await harness.Sender.Send(new NextRoundCommand(roomId, hostId));
        }

        await harness.Sender.Send(new RequestRematchCommand(roomId, hostId, true));
        var secondMatch = await harness.Matches.GetActiveByRoomIdAsync(roomId);
        Assert.Null(secondMatch); // only the host is ready so far

        await harness.Sender.Send(new RequestRematchCommand(roomId, guestId, true));
        secondMatch = await harness.Matches.GetActiveByRoomIdAsync(roomId);

        Assert.NotNull(secondMatch);
        Assert.NotEqual(firstMatch.Id, secondMatch!.Id);
    }

    [Fact]
    public async Task SubmitDefense_from_previous_connection_is_rejected()
    {
        var (harness, roomId, hostId, _) = await StartTwoPlayerMatchAsync();
        var room = await harness.Rooms.GetByIdAsync(roomId);
        var hostToken = room!.Players.Single(p => p.Id == hostId).ReconnectToken;
        room.Disconnect(hostId);
        await harness.Sender.Send(new RejoinCommand(room.Code.Value, hostToken, "conn-new"));

        await Assert.ThrowsAsync<ConnectionNotAuthorizedException>(() => harness.Sender.Send(
            new SubmitDefenseCommand(roomId, hostId, "Defesa antiga", "conn-host")));
    }
}
