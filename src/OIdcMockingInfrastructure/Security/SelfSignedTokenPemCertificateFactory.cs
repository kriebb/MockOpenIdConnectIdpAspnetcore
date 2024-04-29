using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OIdcMockingInfrastructure.Security;

public static class SelfSignedTokenPemCertificateFactory
{
    public static PemCertificate Create()
    {
        using (RSA rsa = RSA.Create())
        {
            // Creating an RSA key pair:
            // RSA (Rivest–Shamir–Adleman) is a public-key cryptosystem that is widely used for secure data transmission.
            // In this class, a 2048-bit RSA key pair is generated. This key pair consists of a public key and a private key.
            // The public key can be freely distributed, while the private key must be kept secret.
        
            rsa.KeySize = 2048;

            // Creating a self-signed certificate:
            // A self-signed certificate is a certificate that is not signed by a trusted certificate authority (CA).
            // Instead, it is signed with its own private key.
            // This is done using the CertificateRequest class in .NET, which allows you to create a new certificate request, and then self-sign it. 
            CertificateRequest request = new CertificateRequest("cn=i.do.not.exist", rsa, HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

             
            // Setting the validity period of the certificate:
            // The certificate is valid from one day in the past to 3650 days in the future.
            // This is done to avoid any potential issues with time synchronization.
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
            {
                new Oid("1.3.6.1.5.5.7.3.1")
            }, false));

            X509Certificate2 cert = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
                new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));

            // Creating a PemCertificate object: This object contains the PEM-formatted certificate and keys. It is returned by the Create method.
            // PEM (Privacy Enhanced Mail) is a widely used format for storing and transmitting cryptographic keys and certificates.

            byte[] certificateBytes = cert.RawData;
            char[] certificatePem = PemEncoding.Write("CERTIFICATE", certificateBytes);

            AsymmetricAlgorithm? key = cert.GetRSAPrivateKey();
            if (key == null) throw new ArgumentNullException(nameof(key));

            byte[] pubKeyBytes = key.ExportSubjectPublicKeyInfo();
            byte[] privKeyBytes = key.ExportPkcs8PrivateKey();
            char[] pubKeyPem = PemEncoding.Write("PUBLIC KEY", pubKeyBytes);
            char[] privKeyPem = PemEncoding.Write("PRIVATE KEY", privKeyBytes);
            //This support is particularly useful in scenarios where using X509 certificates can be challenging, such as on a build server.
            //When using X509 certificates, the certificate is often loaded into a user profile's certificate store.
             //One of these limitations is that it requires a user profile to be loaded.
            //PEM files do not require a user profile or access to a certificate store.
            
            //In ASP.NET Core, you can use the X509Certificate2 class to load a certificate from a PEM file.
            //This can be done entirely in memory, without the need for a user profile or access to a certificate store.

            var pemCertificate = new PemCertificate(
                Certificate: new string(certificatePem),
                PublicKey: new string(pubKeyPem),
                PrivateKey: new string(privKeyPem)
            );

            return pemCertificate;
        }
    }
}