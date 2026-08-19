using System.Collections.Concurrent;

namespace AbsurdCourt.Api.RealTime;

public sealed class PlayerSessionStore
{
    private readonly ConcurrentDictionary<string, Session> sessions = new();

    public void Set(string sessionId, string roomCode, Guid reconnectToken) =>
        sessions[sessionId] = new Session(roomCode, reconnectToken, DateTimeOffset.UtcNow.AddDays(30));

    public bool TryGet(string sessionId, out Session session)
    {
        if (sessions.TryGetValue(sessionId, out session!) && session.ExpiresAt > DateTimeOffset.UtcNow)
            return true;

        sessions.TryRemove(sessionId, out _);
        session = null!;
        return false;
    }

    public void Remove(string sessionId) => sessions.TryRemove(sessionId, out _);

    public sealed record Session(string RoomCode, Guid ReconnectToken, DateTimeOffset ExpiresAt);
}
