using System.Collections.Specialized;
using ConcertApp.Tests.Controllers;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ConcertApp.Tests.Infrastructure.OpenId;

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