using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace WeatherApp.Demo2.Tests.Infrastructure.Security;

public static class SelfSignedAccessTokenPemCertificateFactory
{
    public static PemCertificate Create()
    {
        using (RSA rsa = RSA.Create())
        {
            // Generate a 2048 bit RSA key pair
            rsa.KeySize = 2048;

            // Create a new self signed certificate
            CertificateRequest request = new CertificateRequest("cn=i.do.not.exist", rsa, HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            // Set the validity period of the certificate
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
            {
                new Oid("1.3.6.1.5.5.7.3.1")
            }, false));

            X509Certificate2 cert = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
                new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));

            // Export the certificate to a PEM file
            byte[] certificateBytes = cert.RawData;
            char[] certificatePem = PemEncoding.Write("CERTIFICATE", certificateBytes);

            AsymmetricAlgorithm? key = cert.GetRSAPrivateKey();
            if (key == null) throw new ArgumentNullException(nameof(key));

            byte[] pubKeyBytes = key.ExportSubjectPublicKeyInfo();
            byte[] privKeyBytes = key.ExportPkcs8PrivateKey();
            char[] pubKeyPem = PemEncoding.Write("PUBLIC KEY", pubKeyBytes);
            char[] privKeyPem = PemEncoding.Write("PRIVATE KEY", privKeyBytes);


            var pemCertificate = new PemCertificate(
                Certificate: new string(certificatePem),
                PublicKey: new string(pubKeyPem),
                PrivateKey: new string(privKeyPem)
            );

            return pemCertificate;
        }
    }
}