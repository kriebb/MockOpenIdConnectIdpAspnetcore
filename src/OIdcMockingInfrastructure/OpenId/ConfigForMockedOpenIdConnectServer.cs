using System.Collections.Specialized;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OIdcMockingInfrastructure.Models;

namespace OIdcMockingInfrastructure.OpenId;

public class ConfigForMockedOpenIdConnectServer
{
    public static IConfigurationManager<OpenIdConnectConfiguration> Create(string validIssuer,
        Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery), Token>
            tokenFactoryFunc, Func<UserInfoEndpointResponseBody>? userInfoResponseFunc)
    {
        var openIdHttpClient = CreateHttpClient(validIssuer, tokenFactoryFunc, userInfoResponseFunc);

        return new ConfigurationManager<OpenIdConnectConfiguration>(
            Consts.WellKnownOpenIdConfiguration, new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(openIdHttpClient));
    }
    
    
    public static HttpClient CreateHttpClient(string validIssuer,
        Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery), Token>
            tokenFactoryFunc, Func<UserInfoEndpointResponseBody>? userInfoResponseFunc)
    {
        var openIdHttpClient = CreateHttpClient(
            CreateHttpHandler(validIssuer, tokenFactoryFunc,userInfoResponseFunc));

        return openIdHttpClient;
    }
    
    public static HttpClient CreateHttpClient(MockingOpenIdProviderMessageHandler handler)
    {
        var openIdHttpClient = new HttpClient(handler);

        return openIdHttpClient;
    }

    public static MockingOpenIdProviderMessageHandler CreateHttpHandler(string validIssuer,
        Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenCode), Token>?
            tokenFactoryFunc, Func<UserInfoEndpointResponseBody>? userInfoResponseFunc)
    {
        return new MockingOpenIdProviderMessageHandler(
            Consts.ValidOpenIdConnectDiscoveryDocumentConfiguration(validIssuer), Consts.ValidSigningCertificate,
            tokenFactoryFunc, userInfoResponseFunc);
        
    }

    public static IConfigurationManager<OpenIdConnectConfiguration>? Create(MockingOpenIdProviderMessageHandler backChannelMessageHandler)
    {
        return new ConfigurationManager<OpenIdConnectConfiguration>(
            Consts.WellKnownOpenIdConfiguration, new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(CreateHttpClient(backChannelMessageHandler)));
    }

    public static IConfigurationManager<OpenIdConnectConfiguration>? Create(string validIssuer)
    {
        var backChannelMessageHandler = new MockingOpenIdProviderMessageHandler(
            Consts.ValidOpenIdConnectDiscoveryDocumentConfiguration(validIssuer), Consts.ValidSigningCertificate);

        return new ConfigurationManager<OpenIdConnectConfiguration>(
            Consts.WellKnownOpenIdConfiguration, new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(CreateHttpClient(backChannelMessageHandler)));
    }
}