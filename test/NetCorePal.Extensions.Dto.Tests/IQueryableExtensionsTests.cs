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

    [Fact]
    public void OrderByIfTests()
    {
        var data = new List<int> { 2, 5, 1, 4, 3 }.AsQueryable();
        var result1 = data.OrderByIf(condition: true, predicate: x => x, desc: false).ToList();
        Assert.Equal(5, result1.Count);
        Assert.Equal(1, result1[0]);
        Assert.Equal(2, result1[1]);
        Assert.Equal(3, result1[2]);
        Assert.Equal(4, result1[3]);
        Assert.Equal(5, result1[4]);

        var result2 = data.OrderByIf(condition: true, predicate: x => x, desc: true).ToList();
        Assert.Equal(5, result2.Count);
        Assert.Equal(5, result2[0]);
        Assert.Equal(4, result2[1]);
        Assert.Equal(3, result2[2]);
        Assert.Equal(2, result2[3]);
        Assert.Equal(1, result2[4]);

        var result3 = data.OrderByIf(condition: false, predicate: x => x, desc: false).ToList();
        Assert.Equal(5, result3.Count);
        Assert.Equal(2, result3[0]);
        Assert.Equal(5, result3[1]);
        Assert.Equal(1, result3[2]);
        Assert.Equal(4, result3[3]);
        Assert.Equal(3, result3[4]);

        var result4 = data.OrderByIf(condition: false, predicate: x => x, desc: true).ToList();
        Assert.Equal(5, result4.Count);
        Assert.Equal(2, result4[0]);
        Assert.Equal(5, result4[1]);
        Assert.Equal(1, result4[2]);
        Assert.Equal(4, result4[3]);
        Assert.Equal(3, result4[4]);
    }

    [Fact]
    public void ThenByIfTests()
    {
        var data = new List<int> { 2, 5, 1, 4, 3 }.AsQueryable();
        var result1 = data.OrderBy(x => 0).ThenByIf(condition: true, predicate: x => x, desc: false).ToList();
        Assert.Equal(5, result1.Count);
        Assert.Equal(1, result1[0]);
        Assert.Equal(2, result1[1]);
        Assert.Equal(3, result1[2]);
        Assert.Equal(4, result1[3]);
        Assert.Equal(5, result1[4]);

        var result2 = data.OrderBy(x => 0).ThenByIf(condition: true, predicate: x => x, desc: true).ToList();
        Assert.Equal(5, result2.Count);
        Assert.Equal(5, result2[0]);
        Assert.Equal(4, result2[1]);
        Assert.Equal(3, result2[2]);
        Assert.Equal(2, result2[3]);
        Assert.Equal(1, result2[4]);

        var result3 = data.OrderBy(x => 0).ThenByIf(condition: false, predicate: x => x, desc: false).ToList();
        Assert.Equal(5, result3.Count);
        Assert.Equal(2, result3[0]);
        Assert.Equal(5, result3[1]);
        Assert.Equal(1, result3[2]);
        Assert.Equal(4, result3[3]);
        Assert.Equal(3, result3[4]);

        var result4 = data.OrderBy(x => 0).ThenByIf(condition: false, predicate: x => x, desc: true).ToList();
        Assert.Equal(5, result4.Count);
        Assert.Equal(2, result4[0]);
        Assert.Equal(5, result4[1]);
        Assert.Equal(1, result4[2]);
        Assert.Equal(4, result4[3]);
        Assert.Equal(3, result4[4]);
    }
}