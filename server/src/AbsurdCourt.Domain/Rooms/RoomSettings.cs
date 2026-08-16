namespace AbsurdCourt.Domain.Rooms;

/// <summary>Host-configurable, chosen in the lobby before a match starts — mirrors the "Casos"/"Tempo" segmented controls in the prototype.</summary>
public sealed record RoomSettings
{
    public static readonly IReadOnlyList<int> AllowedCaseCounts = [3, 5, 10];
    public static readonly IReadOnlyList<int> AllowedRoundDurationsSeconds = [15, 30, 45];

    public int CaseCount { get; }
    public int RoundDurationSeconds { get; }

    private RoomSettings(int caseCount, int roundDurationSeconds)
    {
        CaseCount = caseCount;
        RoundDurationSeconds = roundDurationSeconds;
    }

    public static RoomSettings Default() => new(3, 15);

    public static RoomSettings Create(int caseCount, int roundDurationSeconds)
    {
        if (!AllowedCaseCounts.Contains(caseCount))
            throw new ArgumentException($"Número de casos deve ser um de: {string.Join(", ", AllowedCaseCounts)}.", nameof(caseCount));
        if (!AllowedRoundDurationsSeconds.Contains(roundDurationSeconds))
            throw new ArgumentException($"Duração da rodada deve ser uma de: {string.Join(", ", AllowedRoundDurationsSeconds)}.", nameof(roundDurationSeconds));

        return new RoomSettings(caseCount, roundDurationSeconds);
    }
}
