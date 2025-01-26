using System.Security.Cryptography;
using System.Text.Json;

namespace NetCorePal.Extensions.Jwt;

public static class SecretKeyGenerator
{
    public static JwtSecretKeySetting GenerateRsaKeys()
    {
        using var rsa = new RSACryptoServiceProvider(2048);
        var rsaParameters = rsa.ExportParameters(true);

        var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

        var key = new JwtSecretKeySetting(privateKey,
            Kid: Guid.NewGuid().ToString().ToLower(),
            Kty: "RSA", Alg: "RS256", Use: "sig",
            N: Convert.ToBase64String(rsaParameters.Modulus!),
            E: Convert.ToBase64String(rsaParameters.Exponent!));
        return key;
    }
}