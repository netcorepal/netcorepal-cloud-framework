using NetCorePal.Extensions.Domain;
using NetCorePal.Web.Domain;
using NetCorePal.Web.Jwt;

namespace NetCorePal.Web.UnitTests;

public class JwtTests
{
    [Fact]
    public void GenerateRsaKeysTest()
    {
        SecretKeyGenerator.GenerateRsaKeys();
    }

    [Fact]
    public void ATest()
    {
        var type = typeof(Order);
        var prop = type.GetProperty("RowVersion")!;

        var order = new Order("ac", 1);
        prop.SetValue(order,new RowVersion(10));

    }
}