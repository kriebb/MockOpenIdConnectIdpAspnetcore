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

        //TODO: 01_Test2
         

        public void Dispose()
        {
            _fixture.ClearOutputHelper();
        }
    }


}
//TODO: 02_0_AccessTokenParameters
