using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Matches;
using Microsoft.EntityFrameworkCore;

namespace AbsurdCourt.Infrastructure.Persistence;

public sealed class MatchRepository(CourtDbContext db) : IMatchRepository
{
    public Task<Match?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Include(db.Matches).FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<Match?> GetActiveByRoomIdAsync(Guid roomId, CancellationToken ct = default) =>
        Include(db.Matches).FirstOrDefaultAsync(m => m.RoomId == roomId && m.Status == MatchStatus.InProgress, ct);

    public void Add(Match match) => db.Matches.Add(match);

    private static IQueryable<Match> Include(IQueryable<Match> query) =>
        query.Include(m => m.Rounds).ThenInclude(r => r.Defenses);
}
