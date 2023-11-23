//DEMOSNIPPETS-TAB Demo3 Application
//DEMOSNIPPETS-LABEL 01_Test3
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