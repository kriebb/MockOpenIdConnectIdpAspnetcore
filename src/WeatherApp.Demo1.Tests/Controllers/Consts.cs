using WeatherApp.Demo.Tests.Infrastructure.OpenId;
using WeatherApp.Demo.Tests.Infrastructure.Security;

namespace WeatherApp.Demo.Tests.Controllers;

public class Consts
{


    public static int BaseAddressPort { get; set; } = 8443;

    public static readonly Uri BaseAddress = new($"https://localhost:{BaseAddressPort}");

    public static PemCertificate ValidSigningCertificate { get; } = SelfSignedAccessTokenPemCertificateFactory.Create();
    public static string ValidIssuer { get; } = $"Issuer:Dotnet:WeatherApp:Tests:Project";
    public static string ValidAudience { get; } = $"Audience:Dotnet:WeatherApp:Project";
    public const string SubClaimType = "sub";
    public const string SubClaimValidValue = "sub-value";

    public static string WellKnownOpenIdConfiguration { get; set; } =
        "https://localhost:6666/.well-known/openid-configuration";

    public static OpenIdConnectDiscoveryDocumentConfiguration
        ValidOpenIdConnectDiscoveryDocumentConfiguration { get; } =
        OpenIdConnectDiscoveryDocumentConfigurationFactory.Create(Consts.ValidIssuer);


    public const string ScopeClaimType = "scope";
    public const string ScopeClaimValidValue = "weatherforecast:read";

    public const string CountryClaimType = "country";
    public const string CountryClaimValidValue = "Belgium";
}


