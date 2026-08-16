using AbsurdCourt.Domain.Matches;

namespace AbsurdCourt.Application.Abstractions;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>The room's currently in-progress match, if any (a room has at most one active match at a time).</summary>
    Task<Match?> GetActiveByRoomIdAsync(Guid roomId, CancellationToken ct = default);

    void Add(Match match);
}
