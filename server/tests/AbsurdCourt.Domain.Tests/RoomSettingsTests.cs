using AbsurdCourt.Domain.Rooms;

namespace AbsurdCourt.Domain.Tests;

public class RoomSettingsTests
{
    [Theory]
    [InlineData(3, 45)]
    [InlineData(5, 45)]
    [InlineData(10, 30)]
    public void Create_accepts_allowed_combinations(int caseCount, int roundDurationSeconds)
    {
        var settings = RoomSettings.Create(caseCount, roundDurationSeconds);

        Assert.Equal(caseCount, settings.CaseCount);
        Assert.Equal(roundDurationSeconds, settings.RoundDurationSeconds);
    }

    [Theory]
    [InlineData(4, 60)]
    [InlineData(3, 40)]
    [InlineData(15, 45)]
    [InlineData(3, 60)]
    public void Create_rejects_values_outside_the_allowed_sets(int caseCount, int roundDurationSeconds) =>
        Assert.Throws<ArgumentException>(() => RoomSettings.Create(caseCount, roundDurationSeconds));

    [Fact]
    public void Default_is_three_cases_fifteen_seconds()
    {
        var settings = RoomSettings.Default();

        Assert.Equal(3, settings.CaseCount);
        Assert.Equal(15, settings.RoundDurationSeconds);
    }
}
