namespace NetCorePal.Extensions.Jwt;

public class JwtSecretKeySetting(string privateKey, 
    string kid, string kty, string alg, string use, string n, string e)
{
    public string PrivateKey { get; private set; } = privateKey;
    public string Kid { get; private set; } = kid;
    public string Kty { get; set; } = kty;
    public string Alg { get; set; } = alg;
    public string Use { get; set; } = use;
    public string N { get; set; } = n;
    public string E { get; set; } = e;
}