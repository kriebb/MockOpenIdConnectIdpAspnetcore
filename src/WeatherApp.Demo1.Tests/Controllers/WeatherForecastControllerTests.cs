using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonConverter.Abstractions;
using JsonConverter.Abstractions.Models;
using Shouldly;
using WeatherApp.Demo.Tests.Infrastructure.Jwt;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Xunit;
using Xunit.Abstractions;

namespace WeatherApp.Demo.Tests.Controllers;

public class WeatherForecastControllerTests(WeatherForecastServerSetupFixture fixture) : IClassFixture<WeatherForecastServerSetupFixture>
{
    public sealed class GetWeatherForecast: WeatherForecastControllerTests,IDisposable
    {
        private const string CountryClaimType = "country";
        private const string CountryClaimInvalidValue = "France";

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

        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithAccessToken_InvalidClaim_ShouldReturn403()
        {
            var accessTokenParameters = new AccessTokenParameters();
            accessTokenParameters.AddOrReplaceClaim(CountryClaimType, CountryClaimInvalidValue);
            var httpClient =
                _fixture.CreateDefaultClient(
                    new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
            var response = await httpClient.GetAsync($"/WeatherForecast/");
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

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

    public void AddOrReplaceClaim(string claimType, string claimValue)
    {
        var claim = Claims?.FirstOrDefault(x => x.Type == claimType);
        if (claim != null)
            Claims?.Remove(claim);

        Claims ??= new List<Claim>();
        Claims.Add(new Claim(claimType, claimValue));
    }
}
    
