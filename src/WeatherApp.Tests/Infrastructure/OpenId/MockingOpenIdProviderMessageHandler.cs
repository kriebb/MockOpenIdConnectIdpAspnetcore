using System.Collections.Concurrent;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using WeatherApp.Tests.Controllers;
using WeatherApp.Tests.Infrastructure.Security;
using WireMock.Types;


namespace WeatherApp.Tests.Infrastructure.OpenId;

public sealed class MockingOpenIdProviderMessageHandler : HttpMessageHandler
{
    private readonly OpenIdConnectDiscoveryDocumentConfiguration _openIdConnectDiscoveryDocumentConfiguration;
    private readonly PemCertificate _tokenSigningCertificate;

    private readonly IDictionary<string?, NameValueCollection> _requests = new ConcurrentDictionary<string?, NameValueCollection>();
    private readonly Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery), (string AccessToken, string IDToken, string RefreshToken)> _tokenFactoryFunc;

    public MockingOpenIdProviderMessageHandler(
        OpenIdConnectDiscoveryDocumentConfiguration openIdConnectDiscoveryDocumentConfiguration,
        PemCertificate tokenSigningCertificate,
        Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery), (string AccessToken, string IDToken, string RefreshToken)> tokenFactoryFunc)
    {
        _openIdConnectDiscoveryDocumentConfiguration = openIdConnectDiscoveryDocumentConfiguration ?? throw new ArgumentNullException(nameof(openIdConnectDiscoveryDocumentConfiguration));
        _tokenSigningCertificate = tokenSigningCertificate ?? throw new ArgumentNullException(nameof(tokenSigningCertificate));
        _tokenFactoryFunc = tokenFactoryFunc;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SendAsync(request, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.RequestUri == null) throw new ArgumentNullException(nameof(request.RequestUri));

        if (request.RequestUri.AbsoluteUri.Contains(Consts.WellKnownOpenIdConfiguration))
            return await GetOpenIdConfigurationHttpResponseMessage();

        if (request.RequestUri.AbsoluteUri.Equals(_openIdConnectDiscoveryDocumentConfiguration.JwksUri))
            return await GetJwksHttpResonseMessage();

        if (request.RequestUri.AbsoluteUri.Contains(_openIdConnectDiscoveryDocumentConfiguration.AuthorizationEndpoint))
            return await GetAuthorizationResponseMessage(request);

        if (request.RequestUri.AbsoluteUri.Contains(_openIdConnectDiscoveryDocumentConfiguration.TokenEndpoint))
            return await GetTokenResponseMessage(request);

        throw new NotSupportedException("I only support mocking jwks.json, openid-configuration, token-endpoint and authorization-endpoint");
    }

    private async Task<HttpResponseMessage> GetTokenResponseMessage(HttpRequestMessage request)
    {
        // Extracting query parameters from the actual request
        var query = request.RequestUri?.Query;
        //parse the query to a dictionary
        if (query == null) throw new ArgumentNullException(nameof(query), "No query parameters found in http request");
        var queryString = HttpUtility.ParseQueryString(query);

        var state = queryString["state"];
        if(state == null)
            throw new ArgumentNullException(nameof(state), "No state found in http request");

        var authorizationCodeQueryString = _requests[state];
        if (authorizationCodeQueryString == null)
            throw new ArgumentNullException(nameof(authorizationCodeQueryString), $"No authorization code querystring found for state {state}");
        

        var generatedTokens = _tokenFactoryFunc(new(authorizationCodeQueryString!, 
            queryString!
        ));

        var message = new HttpResponseMessage(HttpStatusCode.OK);
        message.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true};
   
        // https://openid.net/specs/openid-connect-core-1_0.html#TokenResponse
        var tokenMessage = new
        {
            access_token = generatedTokens.AccessToken,
            token_type = "Bearer",
            expires_in = 3600,
            refresh_token = generatedTokens.RefreshToken,
            id_token = generatedTokens.IDToken

        };
        message.Content = JsonContent.Create(tokenMessage, mediaType: MediaTypeHeaderValue.Parse("application/json"));


        return message;
    }

    private async Task<HttpResponseMessage> GetAuthorizationResponseMessage(HttpRequestMessage request)
    {
        // Extracting query parameters from the actual request
        var query = request.RequestUri?.Query;
        //parse the query to a dictionary
        if (query == null) throw new ArgumentNullException(nameof(query), "No query parameters found in http request");

        var queryString = HttpUtility.ParseQueryString(query);

        var redirectUri = queryString["redirect_uri"];
        if (redirectUri == null)
            throw new ArgumentNullException(nameof(redirectUri), "No redirect_uri found in http request");
        var state = queryString["state"];
        if (state == null)
            throw new ArgumentNullException(nameof(state), "No state found in http request");
        var nonce = queryString["nonce"];
        if (nonce == null)
            throw new ArgumentNullException(nameof(nonce), "No nonce found in http request. Needed to build the id token later when calling the token endpoint");
        var scope = queryString["scope"];
        if (scope == null)
            throw new ArgumentNullException(nameof(nonce), "No scope found in http request. Needed to build the tokenresponse");
        // State is used as a reference to retrieve the query parameters later

        _requests.Add(state, queryString);
        // Assuming the captured redirect_uri is already URL-encoded (as it should be)
        // Build the Location header with the captured redirect_uri
        // The code is are hardcoded for simplicity
        string locationHeader = Uri.UnescapeDataString(redirectUri);
        locationHeader += $"?code={Consts.AuthorizationCode}&state={state}"; //https://openid.net/specs/openid-connect-core-1_0.html#AuthResponse

        // Provide the response with the redirection status and headers
        var message = new HttpResponseMessage(HttpStatusCode.Redirect);
        message.Headers.Location =new Uri(locationHeader); // Redirect back to the client with the authorization code and the state

        return message;
    }

    private Task<HttpResponseMessage> GetOpenIdConfigurationHttpResponseMessage()
    {

        var httpResponseMessage = new HttpResponseMessage();

        httpResponseMessage.StatusCode = HttpStatusCode.OK;
        httpResponseMessage.Content = JsonContent.Create(_openIdConnectDiscoveryDocumentConfiguration,MediaTypeHeaderValue.Parse("application/json"));
        return Task.FromResult(httpResponseMessage);
    }

    private Task<HttpResponseMessage> GetJwksHttpResonseMessage()
    {
        var httpResponseMessage = new HttpResponseMessage();

        var jwksCertificate = _tokenSigningCertificate.ToJwksCertificate();
        httpResponseMessage.Content = JsonContent.Create(jwksCertificate, MediaTypeHeaderValue.Parse("application/json"));

        httpResponseMessage.StatusCode = HttpStatusCode.OK;
        return Task.FromResult(httpResponseMessage);
    }
    

   
}