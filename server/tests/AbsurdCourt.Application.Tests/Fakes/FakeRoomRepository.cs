using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Rooms;

namespace AbsurdCourt.Application.Tests.Fakes;

public sealed class FakeRoomRepository : IRoomRepository
{
    private readonly Dictionary<Guid, Room> _byId = new();

    public Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken ct = default) =>
        Task.FromResult(_byId.Values.FirstOrDefault(r => r.Code == code));

    public Task<bool> ExistsByCodeAsync(RoomCode code, CancellationToken ct = default) =>
        Task.FromResult(_byId.Values.Any(r => r.Code == code));

    public void Add(Room room) => _byId[room.Id] = room;
}
