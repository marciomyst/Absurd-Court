using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AbsurdCourt.Infrastructure.Persistence;

/// <summary>
/// SQLite has no native array/JSON column type EF can bind a List/HashSet&lt;Guid&gt; to
/// automatically, so private collection fields (Match's case sequence, Room's rematch-ready
/// set) are stored as delimited strings instead of standing up a join table for what's
/// really just an ordered/unordered set of ids.
/// </summary>
internal static class GuidCollectionConverters
{
    public static ValueConverter<List<Guid>, string> ListConverter { get; } = new(
        v => string.Join(',', v),
        v => string.IsNullOrEmpty(v) ? new List<Guid>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList());

    public static ValueComparer<List<Guid>> ListComparer { get; } = new(
        (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
        v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x)),
        v => v.ToList());

    public static ValueConverter<HashSet<Guid>, string> SetConverter { get; } = new(
        v => string.Join(',', v),
        v => string.IsNullOrEmpty(v) ? new HashSet<Guid>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToHashSet());

    public static ValueComparer<HashSet<Guid>> SetComparer { get; } = new(
        (a, b) => (a ?? new()).SetEquals(b ?? new()),
        v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x)),
        v => v.ToHashSet());
}
