using System.Net.Http.Headers;
using WeatherApp.Tests.Controllers.Models;
using Xunit.Abstractions;

namespace WeatherApp.Tests.Infrastructure.Jwt;

public class JwtBearerCustomAccessTokenHandler(AccessTokenParameters accessTokenParameters,
        ITestOutputHelper testOutputHelper)
    : DelegatingHandler
{
    private readonly AccessTokenParameters _accessTokenParameters = accessTokenParameters ?? throw new ArgumentNullException(nameof(accessTokenParameters));

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var encodedAccessToken = JwtBearerAccessTokenFactory.Create(_accessTokenParameters);
        testOutputHelper.WriteLine("Generated the following encoded accesstoken");
        testOutputHelper.WriteLine(encodedAccessToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", encodedAccessToken);
        return base.Send(request, cancellationToken);
    }

   
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var encodedAccessToken = JwtBearerAccessTokenFactory.Create(_accessTokenParameters);
        testOutputHelper.WriteLine("Generated the following encoded accesstoken");
        testOutputHelper.WriteLine(encodedAccessToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", encodedAccessToken);
        return base.SendAsync(request, cancellationToken);
    }
}