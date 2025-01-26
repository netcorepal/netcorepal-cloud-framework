using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Jwt;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.UnitTests;

public class JwtTests
{
    [Fact]
    public void GenerateRsaKeysTest()
    {
        SecretKeyGenerator.GenerateRsaKeys();
    }
}