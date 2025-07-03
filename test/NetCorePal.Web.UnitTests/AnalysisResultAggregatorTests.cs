using NetCorePal.Extensions.CodeAnalysis;
namespace NetCorePal.Web.UnitTests;

public class AnalysisResultAggregatorTests
{
    [Fact]
    public void Aggregate_ShouldCombineResults()
    {
        var result = AnalysisResultAggregator.Aggregate(
            typeof(Program).Assembly);

        // Assert
        Assert.NotEmpty(result.Entities);
        Assert.NotEmpty(result.DomainEvents);
        Assert.NotEmpty(result.DomainEventHandlers);
        Assert.NotEmpty(result.IntegrationEvents);
        Assert.NotEmpty(result.IntegrationEventHandlers);
        Assert.NotEmpty(result.IntegrationEventConverters);
        Assert.NotEmpty(result.Relationships);
    }
}