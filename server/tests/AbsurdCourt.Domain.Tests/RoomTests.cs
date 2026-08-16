using AbsurdCourt.Domain.Common;
using AbsurdCourt.Domain.Rooms;

namespace AbsurdCourt.Domain.Tests;

public class RoomTests
{
    private static readonly DateTime Now = new(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_adds_host_as_first_player()
    {
        var (room, host) = Room.Create(RoomCode.Generate(), "Marcio", "conn-1", Now);

        Assert.Single(room.Players);
        Assert.True(host.IsHost);
        Assert.Equal(host.Id, room.HostPlayerId);
        Assert.Equal(RoomStatus.Lobby, room.Status);
    }

    [Fact]
    public void AddPlayer_beyond_capacity_throws()
    {
        var (room, _) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);
        for (var i = 1; i < Room.MaxPlayers; i++)
            room.AddPlayer($"Player{i}", $"conn-{i}", Now);

        Assert.Throws<RoomFullException>(() => room.AddPlayer("Overflow", "conn-x", Now));
    }

    [Fact]
    public void AddPlayer_after_match_started_throws()
    {
        var (room, _) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);
        room.AddPlayer("Guest", "conn-1", Now);
        room.BeginMatch();

        Assert.Throws<RoomNotJoinableException>(() => room.AddPlayer("Latecomer", "conn-2", Now));
    }

    [Fact]
    public void BeginMatch_with_single_player_throws()
    {
        var (room, _) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);

        Assert.Throws<NotEnoughPlayersException>(room.BeginMatch);
    }

    [Fact]
    public void Rejoin_with_wrong_token_throws()
    {
        var (room, _) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);

        Assert.Throws<InvalidReconnectTokenException>(() => room.Rejoin(Guid.NewGuid(), "conn-new", Now));
    }

    [Fact]
    public void Rejoin_with_correct_token_reattaches_connection()
    {
        var (room, host) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);
        room.Disconnect(host.Id);

        var reconnected = room.Rejoin(host.ReconnectToken, "conn-new", Now.AddMinutes(1));

        Assert.Equal(host.Id, reconnected.Id);
        Assert.True(reconnected.IsConnected);
        Assert.Equal("conn-new", reconnected.ConnectionId);
    }

    [Fact]
    public void Rejoin_rotates_token_and_old_connection_is_no_longer_current()
    {
        var (room, host) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);
        var oldToken = host.ReconnectToken;
        room.Disconnect(host.Id);

        var reconnected = room.Rejoin(oldToken, "conn-new", Now.AddMinutes(1));

        Assert.NotEqual(oldToken, reconnected.ReconnectToken);
        Assert.True(room.IsCurrentConnection(host.Id, "conn-new"));
        Assert.False(room.IsCurrentConnection(host.Id, "conn-0"));
    }

    [Fact]
    public void Rejoin_with_expired_token_throws()
    {
        var (room, host) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);
        room.Disconnect(host.Id);

        Assert.Throws<InvalidReconnectTokenException>(() => room.Rejoin(host.ReconnectToken, "conn-new", Now.AddDays(31)));
    }

    [Fact]
    public void Rematch_before_a_completed_match_throws()
    {
        var (room, host) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);

        Assert.Throws<RematchNotAvailableException>(() => room.SetRematchReady(host.Id, true));
    }

    [Fact]
    public void SetRematchReady_all_connected_players_ready_returns_true()
    {
        var (room, host) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);
        var guest = room.AddPlayer("Guest", "conn-1", Now);
        room.BeginMatch();
        room.EndMatch(Now);

        room.SetRematchReady(host.Id, true);
        var allReady = room.SetRematchReady(guest.Id, true);

        Assert.True(allReady);
    }

    [Fact]
    public void SetRematchReady_partial_returns_false()
    {
        var (room, host) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);
        room.AddPlayer("Guest", "conn-1", Now);
        room.BeginMatch();
        room.EndMatch(Now);

        var allReady = room.SetRematchReady(host.Id, true);

        Assert.False(allReady);
    }

    [Fact]
    public void UpdateSettings_after_match_started_throws()
    {
        var (room, _) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);
        room.AddPlayer("Guest", "conn-1", Now);
        room.BeginMatch();

        Assert.Throws<RoomNotJoinableException>(() => room.UpdateSettings(5, 45));
    }

    [Fact]
    public void UpdateSettings_while_in_lobby_applies()
    {
        var (room, _) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);

        room.UpdateSettings(10, 30);

        Assert.Equal(10, room.Settings.CaseCount);
        Assert.Equal(30, room.Settings.RoundDurationSeconds);
    }

    [Fact]
    public void HasRematchTimedOut_before_window_elapses_is_false()
    {
        var (room, host) = Room.Create(RoomCode.Generate(), "Host", "conn-0", Now);
        room.AddPlayer("Guest", "conn-1", Now);
        room.BeginMatch();
        room.EndMatch(Now);

        Assert.False(room.HasRematchTimedOut(Now.AddSeconds(10), TimeSpan.FromSeconds(30)));
        Assert.True(room.HasRematchTimedOut(Now.AddSeconds(31), TimeSpan.FromSeconds(30)));
    }
}
