using AbsurdCourt.Domain.Rooms;

namespace AbsurdCourt.Application.Common;

public static class ConnectionAuthorization
{
    public static void EnsureCurrentConnection(this Room room, Guid playerId, string connectionId)
    {
        if (!room.IsCurrentConnection(playerId, connectionId))
            throw new ConnectionNotAuthorizedException(playerId);
    }
}
