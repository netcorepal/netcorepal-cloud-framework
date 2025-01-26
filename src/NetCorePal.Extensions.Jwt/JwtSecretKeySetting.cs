namespace NetCorePal.Extensions.Jwt;

public record JwtSecretKeySetting(
    string PrivateKey,
    string Kid,
    string Kty,
    string Alg,
    string Use,
    string N,
    string E);