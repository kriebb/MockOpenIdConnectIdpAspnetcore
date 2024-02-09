using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WeatherApp.Demo.Tests.Controllers;

namespace WeatherApp.Demo.Tests.Infrastructure.OpenId;
public class ConfigForMockedOpenIdConnectServer
{
    public static IConfigurationManager<OpenIdConnectConfiguration> Create()
    {
        var openIdHttpClient = new HttpClient(
            new MockingOpenIdProviderMessageHandler(Consts.ValidOpenIdConnectDiscoveryDocumentConfiguration, Consts.ValidSigningCertificate));

        return new ConfigurationManager<OpenIdConnectConfiguration>(
            Consts.WellKnownOpenIdConfiguration, new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(openIdHttpClient));
    }

}
