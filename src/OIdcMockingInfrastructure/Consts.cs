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
    
    public static PemCertificate ValidSigningCertificate { get;  }= SelfSignedTokenPemCertificateFactory.Create();

    public static string Authority = "https://i.do.not.exist/";
    public static string WellKnownOpenIdConfiguration { get; set; } = Authority+".well-known/openid-configuration";

    public const string AuthorizationCode = "123456789";
    
    
}