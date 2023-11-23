using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WeatherApp.Demo2.Tests.Controllers;
using WeatherApp.Demo2.Tests.Infrastructure.Security;

namespace WeatherApp.Demo2.Tests.Infrastructure.OpenId;
public sealed class MockingOpenIdProviderMessageHandler(
        OpenIdConnectDiscoveryDocumentConfiguration openIdConnectDiscoveryDocumentConfiguration,
        PemCertificate tokenSigningCertificate)
    : HttpMessageHandler
{
    private readonly OpenIdConnectDiscoveryDocumentConfiguration _openIdConnectDiscoveryDocumentConfiguration = openIdConnectDiscoveryDocumentConfiguration ?? throw new ArgumentNullException(nameof(openIdConnectDiscoveryDocumentConfiguration));
    private readonly PemCertificate _tokenSigningCertificate = tokenSigningCertificate ?? throw new ArgumentNullException(nameof(tokenSigningCertificate));

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

        throw new NotSupportedException("I only support mocking jwks.json and openid-configuration");
    }

    private Task<HttpResponseMessage> GetOpenIdConfigurationHttpResponseMessage()
    {
        var httpResponseMessage = new HttpResponseMessage();
        httpResponseMessage.StatusCode = HttpStatusCode.OK;
        httpResponseMessage.Content = JsonContent.Create(_openIdConnectDiscoveryDocumentConfiguration, MediaTypeHeaderValue.Parse("application/json"));
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