
1. Introduce the application
    - Mention Get operation
    - Explain: Returns a random current Temparature.
    - Explain: Get Operation is protected on class level and on operation level using the Authorize Attribute.
    - Explain: Navigate to the Program.cs and mention the AuthenticationMiddleware that will make sure that the Authorize attribute is used 
        - and the policy is used by the authorization middleware


WeatherForecastControllerTests
```
        [Fact()]
        public async Task WhenWeGetWeatherForecast_WithoutAccessToken_ShouldReturn401()
        {
            var httpClient = _fixture.CreateDefaultClient();
            var response = await httpClient.GetAsync("WeatherForecast");

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
```

