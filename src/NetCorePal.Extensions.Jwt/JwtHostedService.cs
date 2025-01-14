using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;

namespace NetCorePal.Extensions.Jwt;

public class JwtHostedService(IDataProtectionProvider dataProtectionProvider, IJwtSecretKeyStore store) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var protector = dataProtectionProvider.CreateProtector("NetCorePal.Web.Jwt.JwtHostedService");
        var data = protector.Protect("Hello, World!");

        JwtDatas._secretKeySettings = await store.GetSecretKeySettings();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}