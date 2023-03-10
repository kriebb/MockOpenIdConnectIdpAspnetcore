using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace WeatherApp.Tests.Infrastructure.Security;

public sealed record PemCertificate(string Certificate, string PrivateKey, string PublicKey)
{
    public X509Certificate2 ToX509Certificate2()
    {
        return X509Certificate2.CreateFromPem(Certificate, PrivateKey);
    }

    public JsonWebKeySet ToJwksCertificate()
    {

        var certificate = X509Certificate2.CreateFromPem(Certificate);

        var keyParameters = certificate.PublicKey.GetRSAPublicKey()?.ExportParameters(false);
        if (!keyParameters.HasValue)
            throw new ArgumentNullException(nameof(keyParameters));

        var e = Base64UrlEncoder.Encode(keyParameters.Value.Exponent);
        var n = Base64UrlEncoder.Encode(keyParameters.Value.Modulus);
        var dict = new Dictionary<string, string>()
        {
            { "e", e },
            { "kty", "RSA" },
            { "n", n }
        };
        var hash = SHA256.Create();
        Byte[] hashBytes =
            hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dict)));
        JsonWebKey jsonWebKey = new JsonWebKey()
        {
            Kid = Base64UrlEncoder.Encode(hashBytes),
            Kty = "RSA",
            E = e,
            N = n
        };
        JsonWebKeySet jsonWebKeySet = new JsonWebKeySet();
        jsonWebKeySet.Keys.Add(jsonWebKey);

        return jsonWebKeySet;
    }
}