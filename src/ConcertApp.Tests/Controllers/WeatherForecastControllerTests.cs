using System.Net;
using ConcertApp.Tests.Controllers.Models;
using ConcertApp.Tests.Infrastructure.Jwt;
using ConcertApp.Tests.Infrastructure.Security;
using Shouldly;
using Xunit.Abstractions;

namespace ConcertApp.Tests.Controllers;

public class WeatherForecastControllerTests : IClassFixture<WeatherForecastServerSetupFixture>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly WeatherForecastServerSetupFixture _fixture;

    public WeatherForecastControllerTests(ITestOutputHelper testOutputHelper, WeatherForecastServerSetupFixture fixture)
    {
        _testOutputHelper = testOutputHelper;
        _fixture = fixture;
    }



    public sealed class GetWeatherForecast : WeatherForecastControllerTests,IDisposable
    {
        private const string InvalidIssuer = "SomeOtherIssuer";

        private const string InvalidAudience = "SomeOtherAudience";

       
        private const string ScopeClaimType = "scope";
        private const string ScopeClaimInvalidValue = "users:read";

        private const string CountryClaimType = "country";
        private const string CountryClaimInvalidValue = "France";


        public GetWeatherForecast(ITestOutputHelper testOutputHelper, WeatherForecastServerSetupFixture fixture) : base(
            testOutputHelper,
            fixture)
        {
            fixture.SetOutputHelper(testOutputHelper);
        }


        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithoutAccessToken_ShouldReturn401()
        {
            var httpClient = _fixture.CreateDefaultClient();
            var response = await httpClient.GetAsync("WeatherForecast");
          
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
        
        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithAccessToken_InvalidAudience_ShouldReturn401()
        {

            var accessTokenParameters = new AccessTokenParameters()
            {
                Audience = InvalidAudience
            };
              
            var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
            


            var response = await httpClient.GetAsync($"/WeatherForecast/");

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

            
        }
        
        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithAccessToken_InvalidIssuer_ShouldReturn401()
        {

            var accessTokenParameters = new AccessTokenParameters()
            {
                Issuer = InvalidIssuer
            };

            var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));

            var response = await httpClient.GetAsync($"/WeatherForecast/");

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

            
        }
        
        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithAccessToken_InvalidSignature_ShouldReturn401()
        {
           var accessTokenParameters = new AccessTokenParameters()
            {
                SigningCertificate = SelfSignedAccessTokenPemCertificateFactory.Create().ToX509Certificate2()
            };

            var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));

            var response = await httpClient.GetAsync($"/WeatherForecast/");
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
        
        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithAccessToken_InvalidScope_ShouldReturn403()
        {
            var accessTokenParameters = new AccessTokenParameters();
            accessTokenParameters.AddOrReplaceClaim(ScopeClaimType, ScopeClaimInvalidValue);


            var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
            var response = await httpClient.GetAsync($"/WeatherForecast/");
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }
        
        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithAccessToken_InvalidClaim_ShouldReturn403()
        {
            var accessTokenParameters = new AccessTokenParameters();
            accessTokenParameters.AddOrReplaceClaim(CountryClaimType, CountryClaimInvalidValue);


            var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
            var response = await httpClient.GetAsync($"/WeatherForecast/");
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

            
        }
        
        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithValidAccessToken_ShouldReturn200()
        {
           var accessTokenParameters = new AccessTokenParameters();


            var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
            var response = await httpClient.GetAsync($"/WeatherForecast/");

         
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }


        public void Dispose()
        {
            _fixture.ClearOutputHelper();
        }
    }
}