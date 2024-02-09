using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Shouldly;
using WeatherApp.Demo2.Tests.Infrastructure.Jwt;
using Xunit;
using Xunit.Abstractions;

namespace WeatherApp.Demo2.Tests.Controllers;

public class WeatherForecastControllerTests(WeatherForecastServerSetupFixture fixture) : IClassFixture<WeatherForecastServerSetupFixture>
{
    public sealed class GetWeatherForecast: WeatherForecastControllerTests,IDisposable
    {
        //INSERT DEMO4: 03_AddInvalidDataInTestsClass

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly WeatherForecastServerSetupFixture _fixture;

        public GetWeatherForecast(ITestOutputHelper testOutputHelper, WeatherForecastServerSetupFixture fixture) : base(fixture)
        {
            _testOutputHelper = testOutputHelper;
            _fixture = fixture;
            _fixture.SetOutputHelper(testOutputHelper);
        }

        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithoutAccessToken_ShouldReturn401()
        {
            var httpClient = _fixture.CreateDefaultClient();
            var response = await httpClient.GetAsync("WeatherForecast");

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithValidAccessToken_ShouldReturn200()
        {
            var accessTokenParameters = new AccessTokenParameters();
            var httpClient =
                _fixture.CreateDefaultClient(
                    new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
            var response = await httpClient.GetAsync($"/WeatherForecast/");
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        //INSERT DEMO4: 01_Test4

        public void Dispose()
        {
            _fixture.ClearOutputHelper();
        }
    }


}

public record AccessTokenParameters
{
    public X509Certificate2 SigningCertificate { get; set; } = Consts.ValidSigningCertificate.ToX509Certificate2();
    public string Audience { get; set; } = Consts.ValidAudience;
    public string Issuer { get; set; } = Consts.ValidIssuer;

    public List<Claim> Claims { get; set; } = new()
    {
        new(Consts.SubClaimType, Consts.SubClaimValidValue),
        new(Consts.ScopeClaimType, Consts.ScopeClaimValidValue),
        new(Consts.CountryClaimType,
            Consts.CountryClaimValidValue)

    };

    //INSERT DEMO4 02_AccessTokenSupportChangeClaim
}

