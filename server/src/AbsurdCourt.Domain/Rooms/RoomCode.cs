using System.Text.RegularExpressions;

namespace AbsurdCourt.Domain.Rooms;

public sealed partial record RoomCode
{
    public string Value { get; }

    private RoomCode(string value) => Value = value;

    public static RoomCode Create(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var normalized = value.ToUpperInvariant();
        if (!ShapeRegex().IsMatch(normalized))
            throw new ArgumentException("O código da sala deve seguir o formato XXXX-XXXX-XXXX.", nameof(value));

        return new RoomCode(normalized);
    }

    /// <summary>Candidate code only — uniqueness against existing rooms is an Application-layer concern.</summary>
    public static RoomCode Generate()
    {
        var hex = Random.Shared.NextInt64(0, 1L << 48).ToString("X12");
        return new RoomCode($"{hex[..4]}-{hex[4..8]}-{hex[8..12]}");
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}$")]
    private static partial Regex ShapeRegex();
}
