using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

public class StatusNameHelperTests
{
    [Fact]
    public void GetRealStatusNameTest()
    {
        /*
           Failed = -1, 
           Scheduled = 0,
           Succeeded = 1,
           Delayed = 2,
           Queued = 3,
         */
        
        Assert.Equal("Failed", StatusNameHelper.GetRealStatusName("failed"));
        Assert.Equal("Scheduled", StatusNameHelper.GetRealStatusName("SCHEDULED"));
        Assert.Equal("Succeeded", StatusNameHelper.GetRealStatusName("succeeded"));
        Assert.Equal("Delayed", StatusNameHelper.GetRealStatusName("DeLaYeD"));
        Assert.Equal("Queued", StatusNameHelper.GetRealStatusName("queued"));
        Assert.Equal(string.Empty, StatusNameHelper.GetRealStatusName("unknown"));
    }
}