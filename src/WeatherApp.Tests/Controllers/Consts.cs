using WeatherApp.Tests.Infrastructure.OpenId;
using WeatherApp.Tests.Infrastructure.Security;

namespace WeatherApp.Tests.Controllers;

public class Consts
{
    public const string SubClaimType = "sub";
    public const string SubClaimValidValue = "sub-value";

    public const string ScopeClaimType = "scope";
    public const string ScopeClaimValidValue = "weatherforecast:read";

    public const string CountryClaimType = "country";
    public const string CountryClaimValidValue = "Belgium";

    public static int BaseAddressPort { get; set; } = 8443;

    public static readonly Uri BaseAddress = new($"https://localhost:{BaseAddressPort}");
    public static string ValidIssuer { get; } = $"Issuer:Dotnet:WeatherApp:Tests:Project";
    public static string ValidAudience { get; }= $"Audience:Dotnet:WeatherApp:Project";
    
    public static OpenIdConnectDiscoveryDocumentConfiguration ValidOpenIdConnectDiscoveryDocumentConfiguration { get; } = OpenIdConnectDiscoveryDocumentConfigurationFactory.Create(Consts.ValidIssuer);
    
    public static PemCertificate ValidSigningCertificate { get;  }= SelfSignedAccessTokenPemCertificateFactory.Create();

    public static string WellKnownOpenIdConfiguration { get; set; } = "https://i.do.not.exist/.well-known/openid-configuration";
}