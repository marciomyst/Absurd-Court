using AbsurdCourt.Domain.Rooms;

namespace AbsurdCourt.Application.Abstractions;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(RoomCode code, CancellationToken ct = default);
    void Add(Room room);
}
