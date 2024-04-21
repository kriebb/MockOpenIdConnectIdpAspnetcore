using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using OIdcMockingInfrastructure.Models;

namespace OIdcMockingInfrastructure.Jwt;

public class JwtBearerCustomAccessTokenHandler : DelegatingHandler
{
    private readonly TokenParameters _tokenParameters;
    private readonly ILogger? _logger;

    public JwtBearerCustomAccessTokenHandler(TokenParameters tokenParameters, ILogger? logger)
    {
        _tokenParameters = tokenParameters ?? throw new ArgumentNullException(nameof(tokenParameters));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SetAuthorizationHeader(request);
        return base.Send(request, cancellationToken);
    }

    private void SetAuthorizationHeader(HttpRequestMessage request)
    {
        var encodedAccessToken = JwtBearerAccessTokenFactory.Create(_tokenParameters);
        _logger.LogInformation("Generated the following encoded accesstoken");
        _logger.LogInformation(encodedAccessToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", encodedAccessToken);
    }


    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SetAuthorizationHeader(request);

        return base.SendAsync(request, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}