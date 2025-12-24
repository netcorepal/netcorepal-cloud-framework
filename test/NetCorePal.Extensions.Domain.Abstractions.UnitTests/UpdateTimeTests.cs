namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests;

public class UpdateTimeTests
{
    [Fact]
    public void ComparisonOperatorsTest()
    {
        var time1 = new UpdateTime(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var time11 = new UpdateTime(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        Assert.True(time1 == time11);
        var time2 = new UpdateTime(new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero));

        Assert.True(time1 < time2);
        Assert.True(time1 <= time2);
        Assert.False(time1 > time2);
        Assert.False(time1 >= time2);

        Assert.True(time2 > time1);
        Assert.True(time2 >= time1);
        Assert.False(time2 < time1);
        Assert.False(time2 <= time1);

        var time3 = new UpdateTime(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        Assert.True(time1 <= time3);
        Assert.True(time1 >= time3);
        Assert.False(time1 < time3);
        Assert.False(time1 > time3);
    }
}