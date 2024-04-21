using System.Collections.Specialized;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace OIdcMockingInfrastructure.OpenId;

public class ConfigForMockedOpenIdConnectServer
{
    public static IConfigurationManager<OpenIdConnectConfiguration> Create(string validIssuer, Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery), (string AccessToken, string IDToken, string RefreshToken)> tokenFactoryFunc)
    {
        var openIdHttpClient = new HttpClient(
            new MockingOpenIdProviderMessageHandler(
                Consts.ValidOpenIdConnectDiscoveryDocumentConfiguration(validIssuer), Consts.ValidSigningCertificate,
                tokenFactoryFunc));

        return new ConfigurationManager<OpenIdConnectConfiguration>(
            Consts.WellKnownOpenIdConfiguration, new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(openIdHttpClient));
    }

}