using System.Net;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WeatherApp.Demo1.Tests.Controllers;

public class WeatherForecastControllerTests(WeatherForecastServerSetupFixture fixture) : IClassFixture<WeatherForecastServerSetupFixture>
{
    public sealed class GetWeatherForecast: WeatherForecastControllerTests,IDisposable
    {
        private readonly WeatherForecastServerSetupFixture _fixture;

        public GetWeatherForecast(ITestOutputHelper testOutputHelper, WeatherForecastServerSetupFixture fixture) : base(fixture)
        {
            _fixture = fixture;
            _fixture.SetOutputHelper(testOutputHelper);
        }
        /*
         *  Demo: Write Test1: Without AccessToken
         *
         * */


        public void Dispose()
        {
            _fixture.ClearOutputHelper();
        }
    }


}