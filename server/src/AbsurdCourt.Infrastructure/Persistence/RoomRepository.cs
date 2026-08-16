using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Rooms;
using Microsoft.EntityFrameworkCore;

namespace AbsurdCourt.Infrastructure.Persistence;

public sealed class RoomRepository(CourtDbContext db) : IRoomRepository
{
    public Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Rooms.Include(r => r.Players).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken ct = default) =>
        db.Rooms.Include(r => r.Players).FirstOrDefaultAsync(r => r.Code == code, ct);

    public Task<bool> ExistsByCodeAsync(RoomCode code, CancellationToken ct = default) =>
        db.Rooms.AnyAsync(r => r.Code == code, ct);

    public void Add(Room room) => db.Rooms.Add(room);
}
