using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using OIdcMockingInfrastructure.Models;
using OIdcMockingInfrastructure.Security;

namespace OIdcMockingInfrastructure.OpenId;

public sealed class MockingOpenIdProviderMessageHandler(
    OpenIdConnectDiscoveryDocumentConfiguration openIdConnectDiscoveryDocumentConfiguration,
    PemCertificate tokenSigningCertificate)
    : HttpMessageHandler
{
    private readonly OpenIdConnectDiscoveryDocumentConfiguration _openIdConnectDiscoveryDocumentConfiguration = openIdConnectDiscoveryDocumentConfiguration ?? throw new ArgumentNullException(nameof(openIdConnectDiscoveryDocumentConfiguration));
    private readonly PemCertificate _tokenSigningCertificate = tokenSigningCertificate ?? throw new ArgumentNullException(nameof(tokenSigningCertificate));

    private readonly IDictionary<string, NameValueCollection> _requests = new ConcurrentDictionary<string, NameValueCollection>();
    private readonly Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery), Token>? _tokenFactoryFunc;
    private readonly Func<UserInfoEndpointResponseBody>? _userInfoResponseFunc;

    public MockingOpenIdProviderMessageHandler(
        OpenIdConnectDiscoveryDocumentConfiguration openIdConnectDiscoveryDocumentConfiguration,
        PemCertificate tokenSigningCertificate,
        Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery), Token>? tokenFactoryFunc,
        Func<UserInfoEndpointResponseBody>? userInfoResponseFunc):this(openIdConnectDiscoveryDocumentConfiguration, tokenSigningCertificate)

    {
        _tokenFactoryFunc = tokenFactoryFunc;
        _userInfoResponseFunc = userInfoResponseFunc;
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
            return await GetJwksHttpResponseMessage();

        if (request.RequestUri.AbsoluteUri.Contains(_openIdConnectDiscoveryDocumentConfiguration.AuthorizationEndpoint))
            return await GetAuthorizationResponseMessage(request);

        if (request.RequestUri.AbsoluteUri.Contains(_openIdConnectDiscoveryDocumentConfiguration.TokenEndpoint))
            return await GetTokenResponseMessage(request);

        if (request.RequestUri.AbsoluteUri.Contains(_openIdConnectDiscoveryDocumentConfiguration.UserinfoEndpoint))
            return await GetUserInformation(request);
        
        throw new NotSupportedException("I only support mocking jwks.json, openid-configuration, token-endpoint and authorization-endpoint");
    }

    private Task<HttpResponseMessage> GetUserInformation(HttpRequestMessage request)
    {
        if(_userInfoResponseFunc == null)   
            throw new ArgumentNullException(nameof(_userInfoResponseFunc), "No user information response function provided");

        var user = _userInfoResponseFunc();
        if(user == null)
            throw new ArgumentNullException(nameof(user), "No user information response provided");
        var httpResponseMessage = new HttpResponseMessage();

        httpResponseMessage.StatusCode = HttpStatusCode.OK;
        httpResponseMessage.Content = JsonContent.Create(user, MediaTypeHeaderValue.Parse("application/json"));
        return Task.FromResult(httpResponseMessage);
    }

    private async Task<HttpResponseMessage> GetTokenResponseMessage(HttpRequestMessage request)
    {
        if (_tokenFactoryFunc == null)
            throw new ArgumentNullException(nameof(_tokenFactoryFunc), "No token factory function provided");

        NameValueCollection authenticationQueryString;

        if (request.Method == HttpMethod.Get) //For OIDC LIbrary
        {
            // Extracting query parameters from the actual request
            var query = request.RequestUri?.Query;
            //parse the query to a dictionary
            if (query == null)
                throw new ArgumentNullException(nameof(query), "No query parameters found in http request");
            authenticationQueryString = HttpUtility.ParseQueryString(query);


        }
        else
        if (request.Method == HttpMethod.Post) // For MicrosoftAccount Libarary
        {
            var bodyContent = await request.Content!.ReadAsStringAsync();

            authenticationQueryString = HttpUtility.ParseQueryString(bodyContent);
        }
        else
        {
            throw new NotSupportedException("TokenRequest should be a GET or a POST");
        }
        var code = authenticationQueryString["code"];

        if (code == null)
            throw new ArgumentNullException(nameof(code), "No code found in http request");


        var authorizationCodeQueryString = _requests[code];
        if (authorizationCodeQueryString == null)
            throw new ArgumentNullException(nameof(authorizationCodeQueryString), $"No authorization code querystring found for code {code}");


        var generatedTokens = _tokenFactoryFunc(new(authorizationCodeQueryString!, 
            authenticationQueryString!
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
            id_token = generatedTokens.IdToken

        };
        message.Content = JsonContent.Create(tokenMessage, mediaType: MediaTypeHeaderValue.Parse("application/json"));


        return message;
    }

 
    public string GetAuthorizationLocationHeaderFromFullUri(string redirectUriToAuthorizationServer)
    {
        //parse the query to a dictionary
        if (redirectUriToAuthorizationServer == null) throw new ArgumentNullException(nameof(redirectUriToAuthorizationServer), "No query parameters found in http request");

        var queryString = HttpUtility.ParseQueryString(redirectUriToAuthorizationServer);

        var redirectUri = queryString["redirect_uri"];
        if (redirectUri == null)
            throw new ArgumentNullException(nameof(redirectUri), "No redirect_uri found in http request");
        
        var state = queryString["state"];
        var scope = queryString["scope"];
        if (scope == null)
            throw new ArgumentNullException(nameof(scope), "No scope found in http request. Needed to build the token response");
        // State is used as a reference to retrieve the query parameters later

        // Assuming the captured redirect_uri is already URL-encoded (as it should be)
        // Build the Location header with the captured redirect_uri
        // The code is are hardcoded for simplicity
        string locationHeader = Uri.UnescapeDataString(redirectUri);
        locationHeader += $"?code={Consts.AuthorizationCode}";
        
        if(!string.IsNullOrWhiteSpace(state))
            locationHeader +=$"&state={state}"; //https://openid.net/specs/openid-connect-core-1_0.html#AuthResponse
        
        _requests.Add(Consts.AuthorizationCode, queryString);

        // Provide the response with the redirection status and headers
        return locationHeader;    
    }

    public async Task<HttpResponseMessage> GetAuthorizationResponseMessage(HttpRequestMessage request)
    {
        // Extracting query parameters from the actual request
        var query = request.RequestUri?.Query!;
        string locationHeader = GetAuthorizationLocationHeaderFromFullUri(query);
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

    private Task<HttpResponseMessage> GetJwksHttpResponseMessage()
    {
        var httpResponseMessage = new HttpResponseMessage();

        var certificate = _tokenSigningCertificate.ToJwksCertificate();
        httpResponseMessage.Content = JsonContent.Create(certificate, MediaTypeHeaderValue.Parse("application/json"));

        httpResponseMessage.StatusCode = HttpStatusCode.OK;
        return Task.FromResult(httpResponseMessage);
    }


   
}