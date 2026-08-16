using AbsurdCourt.Domain.Rooms;

namespace AbsurdCourt.Domain.Tests;

public class RoomCodeTests
{
    [Theory]
    [InlineData("A410-9032-00FF")]
    [InlineData("0000-0000-0000")]
    public void Create_accepts_valid_shape(string value) =>
        Assert.Equal(value, RoomCode.Create(value).Value);

    [Theory]
    [InlineData("44-903")]
    [InlineData("4419-0302")]
    [InlineData("ABC-123")]
    [InlineData("")]
    public void Create_rejects_invalid_shape(string value) =>
        Assert.Throws<ArgumentException>(() => RoomCode.Create(value));

    [Fact]
    public void Generate_produces_valid_shape()
    {
        for (var i = 0; i < 50; i++)
            RoomCode.Create(RoomCode.Generate().Value);
    }
}
