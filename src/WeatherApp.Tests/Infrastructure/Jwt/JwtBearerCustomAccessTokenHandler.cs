using System.Net.Http.Headers;
using WeatherApp.Tests.Controllers.Models;
using Xunit.Abstractions;

namespace WeatherApp.Tests.Infrastructure.Jwt;

public class JwtBearerCustomAccessTokenHandler : DelegatingHandler
{
    private readonly AccessTokenParameters _accessTokenParameters;
    private readonly ITestOutputHelper _testOutputHelper;

    public JwtBearerCustomAccessTokenHandler(AccessTokenParameters accessTokenParameters, ITestOutputHelper testOutputHelper)
    {
        _accessTokenParameters = accessTokenParameters ?? throw new ArgumentNullException(nameof(accessTokenParameters));
        _testOutputHelper = testOutputHelper;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var encodedAccessToken = JwtBearerAccessTokenFactory.Create(_accessTokenParameters);
        _testOutputHelper.WriteLine("Generated the following encoded accesstoken");
        _testOutputHelper.WriteLine(encodedAccessToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", encodedAccessToken);
        return base.Send(request, cancellationToken);
    }

   
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var encodedAccessToken = JwtBearerAccessTokenFactory.Create(_accessTokenParameters);
        _testOutputHelper.WriteLine("Generated the following encoded accesstoken");
        _testOutputHelper.WriteLine(encodedAccessToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", encodedAccessToken);
        return base.SendAsync(request, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}