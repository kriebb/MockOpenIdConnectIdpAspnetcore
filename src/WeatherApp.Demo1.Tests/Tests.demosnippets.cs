//DEMOSNIPPETS-TAB Demo4 Application
//DEMOSNIPPETS-LABEL 01_Test4
[Fact()]
public async Task WhenWeGetWeatherForecast_WithAccessToken_InvalidClaim_ShouldReturn403()
{
    var accessTokenParameters = new AccessTokenParameters();
    accessTokenParameters.AddOrReplaceClaim(CountryClaimType, CountryClaimInvalidValue);
    var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
    var response = await httpClient.GetAsync($"/WeatherForecast/");
    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
}
//DEMOSNIPPETS-LABEL 02_AccessTokenSupportChangeClaim
public void AddOrReplaceClaim(string claimType, string claimValue)
{
    var claim = Claims?.FirstOrDefault(x => x.Type == claimType);
    if (claim != null)
        Claims?.Remove(claim);

    Claims ??= new List<Claim>();
    Claims.Add(new Claim(claimType, claimValue));
}

//DEMOSNIPPETS-LABEL 03_AddInvalidDataInTestsClass
private const string CountryClaimType = "country";
private const string CountryClaimInvalidValue = "France";
