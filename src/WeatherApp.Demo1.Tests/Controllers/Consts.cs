using WeatherApp.Demo2.Tests.Infrastructure.OpenId;
using WeatherApp.Demo2.Tests.Infrastructure.Security;

namespace WeatherApp.Demo2.Tests.Controllers;

public class Consts
{


    public static int BaseAddressPort { get; set; } = 8443;

    public static readonly Uri BaseAddress = new($"https://localhost:{BaseAddressPort}");

    //Demo 2 INSERT BELOW
    public static PemCertificate ValidSigningCertificate { get; } = SelfSignedAccessTokenPemCertificateFactory.Create();
    public static string ValidIssuer { get; } = $"Issuer:Dotnet:WeatherApp:Tests:Project";
    public static string ValidAudience { get; } = $"Audience:Dotnet:WeatherApp:Project";
    public const string SubClaimType = "sub";
    public const string SubClaimValidValue = "sub-value";

    //Demo 2 INSERT BELOW
    public static string WellKnownOpenIdConfiguration { get; set; } = "https://i.do.not.exist/.well-known/openid-configuration";
    public static OpenIdConnectDiscoveryDocumentConfiguration ValidOpenIdConnectDiscoveryDocumentConfiguration { get; } = OpenIdConnectDiscoveryDocumentConfigurationFactory.Create(Consts.ValidIssuer);

    //Demo 2 INSERT BELOW
//.Tests Consts.cs
public const string ScopeClaimType = "scope";
public const string ScopeClaimValidValue = "weatherforecast:read";

public const string CountryClaimType = "country";
public const string CountryClaimValidValue = "Belgium";
}


