using System.Security.Cryptography;
using System.Text.Json;

namespace NetCorePal.Web.Jwt;

public class SecretKeyGenerator
{
    public static JwtSecretKeySetting GenerateRsaKeys()
    {
        using var rsa = new RSACryptoServiceProvider(2048);
        var rsaParameters = rsa.ExportParameters(true);

        var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

        var key = new JwtSecretKeySetting(privateKey,
            kid: Guid.NewGuid().ToString().ToLower(),
            kty: "RSA", alg: "RS256", use: "sig",
            n: Convert.ToBase64String(rsaParameters.Modulus!),
            e: Convert.ToBase64String(rsaParameters.Exponent!));
        return key;
    }
}