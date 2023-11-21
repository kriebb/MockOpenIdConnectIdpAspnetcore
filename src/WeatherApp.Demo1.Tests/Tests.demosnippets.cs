//DEMOSNIPPETS-TAB Demo1_2_WeatherForecastControllerTests

//DEMOSNIPPETS-LABEL TestWithoutBearerToken
[Fact()]
        public async Task WhenWeGetWeatherForecast_WithoutAccessToken_ShouldReturn401()
        {
            var httpClient = _fixture.CreateDefaultClient();
            var response = await httpClient.GetAsync("WeatherForecast");

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }


