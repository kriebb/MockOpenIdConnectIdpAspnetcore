using System.Collections.Specialized;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WeatherApp.Tests.Controllers;

namespace WeatherApp.Tests.Infrastructure.OpenId;

public class ConfigForMockedOpenIdConnectServer
{
    public static IConfigurationManager<OpenIdConnectConfiguration> Create(Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery), (string AccessToken, string IDToken, string RefreshToken)> tokenFactoryFunc)
    {
        var openIdHttpClient = new HttpClient(
            new MockingOpenIdProviderMessageHandler(
                Consts.ValidOpenIdConnectDiscoveryDocumentConfiguration, Consts.ValidSigningCertificate,
                tokenFactoryFunc));

        return new ConfigurationManager<OpenIdConnectConfiguration>(
            Consts.WellKnownOpenIdConfiguration, new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(openIdHttpClient));
    }

}