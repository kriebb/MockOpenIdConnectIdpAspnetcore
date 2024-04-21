using OIdcMockingInfrastructure.OpenId;
using OIdcMockingInfrastructure.Security;

namespace OIdcMockingInfrastructure;

public class Consts
{
    public const string SubClaimType = "sub";

    public const string ScopeClaimType = "scope";

    public const string CountryClaimType = "country";

    public static int BaseAddressPort { get; set; } = 8443;

    public static readonly Uri BaseAddress = new($"https://localhost:{BaseAddressPort}");

    private static OpenIdConnectDiscoveryDocumentConfiguration? _openIdConnectDiscoveryDocumentConfiguration = null;
    public static OpenIdConnectDiscoveryDocumentConfiguration ValidOpenIdConnectDiscoveryDocumentConfiguration(string validIssuer) => _openIdConnectDiscoveryDocumentConfiguration ??= OpenIdConnectDiscoveryDocumentConfigurationFactory.Create(validIssuer);
    
    public static PemCertificate ValidSigningCertificate { get;  }= SelfSignedAccessTokenPemCertificateFactory.Create();

    public static string WellKnownOpenIdConfiguration { get; set; } = "https://i.do.not.exist/.well-known/openid-configuration";

    public const string AuthorizationCode = "123456789";
}