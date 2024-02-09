using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace WeatherApp.Demo.Tests.Infrastructure.Security;

public sealed record PemCertificate(string Certificate, string PrivateKey, string PublicKey)
{
    /// <summary>
    /// For Signing the JWT
    /// </summary>
    /// <returns></returns>
    public X509Certificate2 ToX509Certificate2()
    {
        return X509Certificate2.CreateFromPem(Certificate, PrivateKey);
    }

    /// <summary>
    /// Converts a PEM certificate to a JSON Web Key Set (JWKS) certificate.
    /// The PEM certificate represents the public key certificate used for authentication and encryption.
    /// The JSON Web Key Set (JWKS) certificate is a standard format that represents a set of JSON Web Keys (JWKs),
    /// which contain the public key information necessary to validate the authenticity and integrity of digital messages.
    /// </summary>
    /// <returns>The JSON Web Key Set (JWKS) certificate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the key parameters are null.</exception>
    public JsonWebKeySet ToJwksCertificate()
    {
        // Convert the PEM certificate to an X509Certificate2 object
        var certificate = X509Certificate2.CreateFromPem(Certificate);

        // Get the RSA public key parameters from the certificate
        var keyParameters = certificate.PublicKey.GetRSAPublicKey()?.ExportParameters(false);
        if (!keyParameters.HasValue)
            throw new ArgumentNullException(nameof(keyParameters));

        // Encode the public key parameters
        var e = Base64UrlEncoder.Encode(keyParameters.Value.Exponent);
        var n = Base64UrlEncoder.Encode(keyParameters.Value.Modulus);

        // Create a dictionary with the public key parameters
        var dict = new Dictionary<string, string>()
        {
            { "e", e },
            { "kty", "RSA" },
            { "n", n }
        };

        // Compute the hash of the dictionary
        var hash = SHA256.Create();
        byte[] hashBytes =
            hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dict)));

        // Create a JSON Web Key (JWK) using the hash as the Key ID (Kid)
        JsonWebKey jsonWebKey = new JsonWebKey()
        {
            Kid = Base64UrlEncoder.Encode(hashBytes),
            Kty = "RSA",
            E = e,
            N = n
        };

        // Create a JSON Web Key Set (JWKS) and add the JWK to it
        JsonWebKeySet jsonWebKeySet = new JsonWebKeySet();
        jsonWebKeySet.Keys.Add(jsonWebKey);

        return jsonWebKeySet;
    }
}