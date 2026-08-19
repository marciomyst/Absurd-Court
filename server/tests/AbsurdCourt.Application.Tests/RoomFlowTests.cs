using AbsurdCourt.Application.Common;
using AbsurdCourt.Application.Features.Matches.StartMatch;
using AbsurdCourt.Application.Features.Rooms.CreateRoom;
using AbsurdCourt.Application.Features.Rooms.JoinRoom;
using AbsurdCourt.Application.Features.Rooms.Rejoin;
using AbsurdCourt.Application.Features.Rooms.UpdateSettings;
using AbsurdCourt.Application.Tests.Fakes;
using AbsurdCourt.Domain.Rooms;

namespace AbsurdCourt.Application.Tests;

public class RoomFlowTests
{
    [Fact]
    public async Task CreateRoom_then_JoinRoom_lists_both_players()
    {
        using var harness = new TestHarness();

        var created = await harness.Sender.Send(new CreateRoomCommand("Marcio", "conn-host"));
        var joined = await harness.Sender.Send(new JoinRoomCommand(created.Room.RoomCode, "Convidado", "conn-guest"));

        Assert.Equal(2, joined.Room.Players.Count);
        Assert.Equal(created.Room.RoomId, joined.Room.RoomId);
    }

    [Fact]
    public async Task JoinRoom_with_unknown_code_throws()
    {
        using var harness = new TestHarness();

        await Assert.ThrowsAsync<RoomNotFoundException>(
            () => harness.Sender.Send(new JoinRoomCommand("0000-0000-0000", "Alguém", "conn-x")));
    }

    [Fact]
    public async Task UpdateSettings_by_non_host_throws()
    {
        using var harness = new TestHarness();
        var created = await harness.Sender.Send(new CreateRoomCommand("Host", "conn-host"));
        var joined = await harness.Sender.Send(new JoinRoomCommand(created.Room.RoomCode, "Guest", "conn-guest"));

        await Assert.ThrowsAsync<NotHostException>(() => harness.Sender.Send(
            new UpdateSettingsCommand(created.Room.RoomId, joined.YourPlayerId, 10, 30)));
    }

    [Fact]
    public async Task StartMatch_by_non_host_throws()
    {
        using var harness = new TestHarness();
        var created = await harness.Sender.Send(new CreateRoomCommand("Host", "conn-host"));
        var joined = await harness.Sender.Send(new JoinRoomCommand(created.Room.RoomCode, "Guest", "conn-guest"));

        await Assert.ThrowsAsync<NotHostException>(() => harness.Sender.Send(
            new StartMatchCommand(created.Room.RoomId, joined.YourPlayerId)));
    }

    [Fact]
    public async Task StartMatch_uses_room_configured_settings()
    {
        using var harness = new TestHarness();
        var created = await harness.Sender.Send(new CreateRoomCommand("Host", "conn-host"));
        await harness.Sender.Send(new JoinRoomCommand(created.Room.RoomCode, "Guest", "conn-guest"));

        await harness.Sender.Send(new UpdateSettingsCommand(created.Room.RoomId, created.YourPlayerId, 5, 45));
        await harness.Sender.Send(new StartMatchCommand(created.Room.RoomId, created.YourPlayerId));

        var match = await harness.Matches.GetActiveByRoomIdAsync(created.Room.RoomId);
        Assert.NotNull(match);
        Assert.Equal(5, match!.CaseCount);
        Assert.Equal(45, match.RoundDurationSeconds);
    }

    [Fact]
    public async Task Rejoin_after_disconnect_restores_player()
    {
        using var harness = new TestHarness();
        var created = await harness.Sender.Send(new CreateRoomCommand("Host", "conn-host"));
        var room = await harness.Rooms.GetByIdAsync(created.Room.RoomId);
        room!.Disconnect(created.YourPlayerId);

        var rejoined = await harness.Sender.Send(
            new RejoinCommand(created.Room.RoomCode, created.ReconnectToken, "conn-new"));

        Assert.Equal(created.YourPlayerId, rejoined.YourPlayerId);
        Assert.True(rejoined.Room.Players.Single(p => p.PlayerId == created.YourPlayerId).IsConnected);
    }

    [Fact]
    public async Task Command_from_previous_connection_is_rejected_after_rejoin()
    {
        using var harness = new TestHarness();
        var created = await harness.Sender.Send(new CreateRoomCommand("Host", "conn-old"));
        var room = await harness.Rooms.GetByIdAsync(created.Room.RoomId);
        room!.Disconnect(created.YourPlayerId);
        await harness.Sender.Send(new RejoinCommand(created.Room.RoomCode, created.ReconnectToken, "conn-new"));

        await Assert.ThrowsAsync<ConnectionNotAuthorizedException>(() => harness.Sender.Send(
            new UpdateSettingsCommand(created.Room.RoomId, created.YourPlayerId, 5, 30, "conn-old")));
    }
}
