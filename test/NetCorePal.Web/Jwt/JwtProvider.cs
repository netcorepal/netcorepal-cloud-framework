using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace NetCorePal.Web.Jwt;

public class JwtProvider
{
    JwtSecretKeyStore _secretKeyStore = new JwtSecretKeyStore();


    public string GenerateJwtToken(JwtData data)
    {
        var keySettings = _secretKeyStore.GetSecretKeySettings().Last();

        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(keySettings.PrivateKey), out _);
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
                ["kid"] = keySettings.Kid
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