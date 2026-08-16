using AbsurdCourt.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbsurdCourt.Domain.Rooms;

public sealed class Player : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Initials { get; private set; } = string.Empty;
    public bool IsHost { get; private set; }
    public string? ConnectionId { get; private set; }
    [NotMapped]
    public Guid ReconnectToken { get; private set; }
    public string ReconnectTokenHash { get; private set; } = string.Empty;
    public DateTime ReconnectTokenExpiresAtUtc { get; private set; }
    public bool IsConnected { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    private Player() { }

    internal Player(Guid id, string name, bool isHost, string connectionId, DateTime joinedAtUtc) : base(id)
    {
        Name = name;
        Initials = ComputeInitials(name);
        IsHost = isHost;
        ConnectionId = connectionId;
        IssueReconnectToken(joinedAtUtc);
        IsConnected = true;
        JoinedAtUtc = joinedAtUtc;
    }

    internal void Reconnect(string connectionId, DateTime nowUtc)
    {
        ConnectionId = connectionId;
        IssueReconnectToken(nowUtc);
        IsConnected = true;
    }

    public bool HasValidReconnectToken(Guid token, DateTime nowUtc) =>
        ReconnectTokenExpiresAtUtc > nowUtc && ReconnectTokenHash == ReconnectTokenHasher.Hash(token);

    private void IssueReconnectToken(DateTime nowUtc)
    {
        ReconnectToken = Guid.NewGuid();
        ReconnectTokenHash = ReconnectTokenHasher.Hash(ReconnectToken);
        ReconnectTokenExpiresAtUtc = nowUtc.AddDays(30);
    }

    internal void Disconnect() => IsConnected = false;

    private static string ComputeInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "??",
            1 => parts[0].Length >= 2 ? parts[0][..2].ToUpperInvariant() : parts[0].ToUpperInvariant(),
            _ => $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
        };
    }
}
