using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace NetCorePal.Extensions.Jwt;

public interface IJwtProvider
{
    ValueTask<string> GenerateJwtToken(JwtData data, CancellationToken cancellationToken = default);
}

public class JwtProvider(IJwtSettingStore settingStore) : IJwtProvider
{
    public async ValueTask<string> GenerateJwtToken(JwtData data, CancellationToken cancellationToken = default)
    {
        var keySettings = (await settingStore.GetSecretKeySettings(cancellationToken)).ToArray();
        if (keySettings == null || !keySettings.Any())
        {
            throw new InvalidOperationException(R.NoSecretKeySettingsFound);
        }

        var setting = keySettings[^1];

        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(setting.PrivateKey), out _);
        var key = new RsaSecurityKey(rsa);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: data.Issuer,
            audience: data.Audience,
            claims: data.Claims,
            notBefore: data.NotBefore,
            expires: data.Expires,
            signingCredentials: credentials
        )
        {
            Header =
            {
                ["kid"] = setting.Kid
            }
        };
        var jwtString = new JwtSecurityTokenHandler().WriteToken(token);
        return jwtString;
    }
}

public record JwtData(
    string Issuer,
    string Audience,
    IEnumerable<Claim> Claims,
    DateTime? NotBefore,
    DateTime? Expires);