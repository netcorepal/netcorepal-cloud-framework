namespace NetCorePal.Extensions.Dto.Tests;

public class IQueryableExtensionsTests
{
    [Fact]
    public void WhereIfTests()
    {
        var data = new List<int> { 1, 2, 3, 4, 5 }.AsQueryable();
        var result = data.WhereIf(true, x => x > 3).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal(4, result[0]);
        Assert.Equal(5, result[1]);

        result = data.WhereIf(false, x => x > 3).ToList();
        Assert.Equal(5, result.Count);
    }
}