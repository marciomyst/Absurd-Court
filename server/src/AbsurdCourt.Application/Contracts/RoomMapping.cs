using AbsurdCourt.Domain.Rooms;

namespace AbsurdCourt.Application.Contracts;

public static class RoomMapping
{
    public static RoomSnapshotDto ToSnapshot(this Room room) => new(
        room.Id,
        room.Code.Value,
        room.Status.ToString(),
        room.HostPlayerId,
        room.Players.Select(p => new PlayerDto(p.Id, p.Name, p.Initials, p.IsHost, p.IsConnected)).ToList(),
        room.Settings.CaseCount,
        room.Settings.RoundDurationSeconds);
}
