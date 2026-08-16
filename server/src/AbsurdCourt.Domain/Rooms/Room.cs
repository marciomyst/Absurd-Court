using AbsurdCourt.Domain.Common;
using AbsurdCourt.Domain.Events;

namespace AbsurdCourt.Domain.Rooms;

public enum RoomStatus { Lobby, InProgress, Ended }

public sealed class Room : AggregateRoot
{
    public const int MaxPlayers = 8;
    private const int MaxNameLength = 24;

    private readonly List<Player> _players = new();
    private readonly HashSet<Guid> _rematchReadyPlayerIds = new();

    public RoomCode Code { get; private set; } = null!;
    public RoomStatus Status { get; private set; }
    public Guid HostPlayerId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public RoomSettings Settings { get; private set; } = RoomSettings.Default();

    /// <summary>Set when a match ends and the room returns to Lobby awaiting rematch votes; cleared once a new match begins. Drives the "room auto-closes if nobody rematches in time" sweep.</summary>
    public DateTime? AwaitingRematchSinceUtc { get; private set; }

    public IReadOnlyList<Player> Players => _players.AsReadOnly();
    public IReadOnlyCollection<Guid> RematchReadyPlayerIds => _rematchReadyPlayerIds;

    public IReadOnlyCollection<Guid> ConnectedPlayerIds =>
        _players.Where(p => p.IsConnected).Select(p => p.Id).ToList();

    public bool IsCurrentConnection(Guid playerId, string connectionId) =>
        _players.Any(p => p.Id == playerId && p.IsConnected && p.ConnectionId == connectionId);

    private Room() { }

    private Room(Guid id, RoomCode code, DateTime nowUtc) : base(id)
    {
        Code = code;
        Status = RoomStatus.Lobby;
        CreatedAtUtc = nowUtc;
    }

    public static (Room Room, Player Host) Create(RoomCode code, string hostName, string hostConnectionId, DateTime nowUtc)
    {
        var room = new Room(Guid.NewGuid(), code, nowUtc);
        var host = room.AddPlayerCore(hostName, isHost: true, hostConnectionId, nowUtc);
        return (room, host);
    }

    public Player AddPlayer(string name, string connectionId, DateTime nowUtc)
    {
        if (Status != RoomStatus.Lobby) throw new RoomNotJoinableException(Id);
        if (_players.Count >= MaxPlayers) throw new RoomFullException(Id);

        return AddPlayerCore(name, isHost: false, connectionId, nowUtc);
    }

    private Player AddPlayerCore(string name, bool isHost, string connectionId, DateTime nowUtc)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length == 0) throw new ArgumentException("O nome do jogador não pode ser vazio.", nameof(name));
        if (trimmed.Length > MaxNameLength) trimmed = trimmed[..MaxNameLength];

        var player = new Player(Guid.NewGuid(), trimmed, isHost, connectionId, nowUtc);
        _players.Add(player);
        if (isHost) HostPlayerId = player.Id;

        Raise(new PlayerJoined(Id, player.Id, player.Name, player.Initials, isHost));
        return player;
    }

    public Player Rejoin(Guid reconnectToken, string connectionId, DateTime nowUtc)
    {
        var player = _players.FirstOrDefault(p => p.HasValidReconnectToken(reconnectToken, nowUtc))
            ?? throw new InvalidReconnectTokenException(Id);

        player.Reconnect(connectionId, nowUtc);
        Raise(new PlayerReconnected(Id, player.Id));
        return player;
    }

    public void Disconnect(Guid playerId)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId);
        if (player is null) return;

        player.Disconnect();
        Raise(new PlayerDisconnected(Id, playerId));
    }

    public void UpdateSettings(int caseCount, int roundDurationSeconds)
    {
        if (Status != RoomStatus.Lobby) throw new RoomNotJoinableException(Id);
        Settings = RoomSettings.Create(caseCount, roundDurationSeconds);
        Raise(new RoomSettingsChanged(Id, Settings.CaseCount, Settings.RoundDurationSeconds));
    }

    public void BeginMatch()
    {
        if (Status != RoomStatus.Lobby) throw new RoomNotJoinableException(Id);
        if (_players.Count < 2) throw new NotEnoughPlayersException(Id);

        _rematchReadyPlayerIds.Clear();
        AwaitingRematchSinceUtc = null;
        Status = RoomStatus.InProgress;
    }

    /// <summary>Back to Lobby: same shape whether this is the very first lobby or the post-match rematch screen — that distinction lives client-side.</summary>
    public void EndMatch(DateTime nowUtc)
    {
        Status = RoomStatus.Lobby;
        AwaitingRematchSinceUtc = nowUtc;
    }

    /// <summary>Used by the idle-room sweeper: has the post-match rematch window elapsed without every connected player opting in?</summary>
    public bool HasRematchTimedOut(DateTime nowUtc, TimeSpan window)
    {
        if (AwaitingRematchSinceUtc is not { } since) return false;
        return nowUtc - since >= window;
    }

    public bool SetRematchReady(Guid playerId, bool ready)
    {
        if (AwaitingRematchSinceUtc is null) throw new RematchNotAvailableException(Id);
        if (!_players.Any(p => p.Id == playerId))
            throw new ArgumentException("Jogador não pertence a esta sala.", nameof(playerId));

        if (ready) _rematchReadyPlayerIds.Add(playerId);
        else _rematchReadyPlayerIds.Remove(playerId);

        var connectedIds = ConnectedPlayerIds;
        var allReady = connectedIds.Count >= 2 && connectedIds.All(_rematchReadyPlayerIds.Contains);

        Raise(new RematchStatusChanged(Id, _rematchReadyPlayerIds.Count, connectedIds.Count, allReady));
        return allReady;
    }

    public void Close(string reason)
    {
        Status = RoomStatus.Ended;
        Raise(new RoomClosed(Id, reason));
    }
}
