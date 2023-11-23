using System.Net.Http.Headers;
using WeatherApp.Demo2.Tests.Controllers;
using Xunit.Abstractions;

namespace WeatherApp.Demo2.Tests.Infrastructure.Jwt;
//Demo 2 INSERT BELOW
public class JwtBearerCustomAccessTokenHandler(AccessTokenParameters accessTokenParameters,
        ITestOutputHelper testOutputHelper)
    : DelegatingHandler
{
    private readonly AccessTokenParameters _accessTokenParameters = accessTokenParameters ?? throw new ArgumentNullException(nameof(accessTokenParameters));

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = BuildBearerHeader(_accessTokenParameters);
        return base.Send(request, cancellationToken);
    }


    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = BuildBearerHeader(_accessTokenParameters);
        return base.SendAsync(request, cancellationToken);
    }

    private AuthenticationHeaderValue BuildBearerHeader(AccessTokenParameters tokenParameters)
    {
        var encodedAccessToken = JwtBearerAccessTokenFactory.Create(tokenParameters);
        testOutputHelper.WriteLine("Generated the following encoded accesstoken");
        testOutputHelper.WriteLine(encodedAccessToken);
        return new AuthenticationHeaderValue("Bearer", encodedAccessToken);
    }
}