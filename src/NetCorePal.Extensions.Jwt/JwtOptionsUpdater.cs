using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace NetCorePal.Extensions.Jwt;

/// <summary>
/// Service for updating JWT Bearer options when keys change
/// </summary>
public class JwtOptionsUpdater : IJwtOptionsUpdater
{
    private readonly IJwtSettingStore _store;
    private readonly IOptionsMonitor<JwtBearerOptions> _optionsMonitor;
    private readonly IPostConfigureOptions<JwtBearerOptions> _postConfigureOptions;

    public JwtOptionsUpdater(
        IJwtSettingStore store,
        IOptionsMonitor<JwtBearerOptions> optionsMonitor,
        IPostConfigureOptions<JwtBearerOptions> postConfigureOptions)
    {
        _store = store;
        _optionsMonitor = optionsMonitor;
        _postConfigureOptions = postConfigureOptions;
    }

    public async Task UpdateOptionsAsync(CancellationToken cancellationToken = default)
    {
        var settings = (await _store.GetSecretKeySettings(cancellationToken)).ToArray();
        
        var options = _optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);
        options.TokenValidationParameters ??= new TokenValidationParameters();
        
        // Include all keys (active and expired) for token validation
        // This allows validating tokens signed with expired keys
        options.TokenValidationParameters.IssuerSigningKeys = settings.Select(x =>
            new RsaSecurityKey(new RSAParameters
            {
                Exponent = Base64UrlEncoder.DecodeBytes(x.E),
                Modulus = Base64UrlEncoder.DecodeBytes(x.N)
            })
            {
                KeyId = x.Kid
            });
            
        _postConfigureOptions.PostConfigure(JwtBearerDefaults.AuthenticationScheme, options);
    }
}