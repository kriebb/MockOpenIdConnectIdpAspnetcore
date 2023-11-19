using WeatherApp.Demo2.Tests.Infrastructure.OpenId;
using WeatherApp.Demo2.Tests.Infrastructure.Security;

namespace WeatherApp.Demo2.Tests.Controllers;

public class Consts
{


    public static int BaseAddressPort { get; set; } = 8443;

    public static readonly Uri BaseAddress = new($"https://localhost:{BaseAddressPort}");

    /*Demo 2 Part 2 */
    public static PemCertificate ValidSigningCertificate { get; } = SelfSignedAccessTokenPemCertificateFactory.Create();
    public static string ValidIssuer { get; } = $"Issuer:Dotnet:WeatherApp:Tests:Project";
    public static string ValidAudience { get; } = $"Audience:Dotnet:WeatherApp:Project";
    public const string SubClaimType = "sub";
    public const string SubClaimValidValue = "sub-value";


    //Demo 2 Part 3 */
    public static OpenIdConnectDiscoveryDocumentConfiguration ValidOpenIdConnectDiscoveryDocumentConfiguration { get; } = OpenIdConnectDiscoveryDocumentConfigurationFactory.Create(Consts.ValidIssuer);
    public static string WellKnownOpenIdConfiguration { get; set; } = "https://i.do.not.exist/.well-known/openid-configuration";


    //Demo 2 Part 4
    public const string ScopeClaimType = "scope";
    public const string ScopeClaimValidValue = "weatherforecast:read";

    public const string CountryClaimType = "country";
    public const string CountryClaimValidValue = "Belgium";

}