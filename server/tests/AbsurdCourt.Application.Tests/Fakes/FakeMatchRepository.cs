using AbsurdCourt.Application.Abstractions;
using AbsurdCourt.Domain.Matches;

namespace AbsurdCourt.Application.Tests.Fakes;

public sealed class FakeMatchRepository : IMatchRepository
{
    private readonly Dictionary<Guid, Match> _byId = new();

    public Task<Match?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public Task<Match?> GetActiveByRoomIdAsync(Guid roomId, CancellationToken ct = default) =>
        Task.FromResult(_byId.Values.FirstOrDefault(m => m.RoomId == roomId && m.Status == MatchStatus.InProgress));

    public void Add(Match match) => _byId[match.Id] = match;
}
