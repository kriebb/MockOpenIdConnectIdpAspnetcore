//DEMOSNIPPETS-TAB Demo2 Application
//DEMOSNIPPETS-LABEL 01_Test2
[Fact()]
public async Task WhenWeGetWeatherForecast_WithValidAccessToken_ShouldReturn200()
{
    var accessTokenParameters = new AccessTokenParameters();
    var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
    var response = await httpClient.GetAsync($"/WeatherForecast/");
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
}
//DEMOSNIPPETS-LABEL 02_AccessTokenParameters
public record AccessTokenParameters
{
    public X509Certificate2 SigningCertificate { get; set; } = Consts.ValidSigningCertificate.ToX509Certificate2();
    public string Audience { get; set; } = Consts.ValidAudience;
    public string Issuer { get; set; } = Consts.ValidIssuer;
    public List<Claim> Claims { get; set; } = new()
    {
        new(Consts.SubClaimType, Consts.SubClaimValidValue)

    };
}
//DEMOSNIPPETS-LABEL 02_a_AccessTokenHandler
public class JwtBearerCustomAccessTokenHandler(AccessTokenParameters accessTokenParameters,
        ITestOutputHelper testOutputHelper)
    : DelegatingHandler
{
    private readonly AccessTokenParameters _accessTokenParameters = accessTokenParameters ?? throw new ArgumentNullException(nameof(accessTokenParameters));

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = BuildBearerHeader(_accessTokenParameters);
        return base.Send(request, cancellationToken);
    }


    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = BuildBearerHeader(_accessTokenParameters);
        return base.SendAsync(request, cancellationToken);
    }

    private AuthenticationHeaderValue BuildBearerHeader(AccessTokenParameters tokenParameters)
    {
        var encodedAccessToken = JwtBearerAccessTokenFactory.Create(tokenParameters);
        testOutputHelper.WriteLine("Generated the following encoded accesstoken");
        testOutputHelper.WriteLine(encodedAccessToken);
        return new AuthenticationHeaderValue("Bearer", encodedAccessToken);
    }
}

//DEMOSNIPPETS-LABEL 02_b_AccessTokenFactory
public class JwtBearerAccessTokenFactory
{
    public static string Create(AccessTokenParameters accessTokenParameters)
    {
        var signingCredentials = new SigningCredentials(new X509SecurityKey(accessTokenParameters.SigningCertificate), SecurityAlgorithms.RsaSha256);

        var notBefore = DateTime.UtcNow;
        var expires = DateTime.UtcNow.AddHours(1);


        var identity = new ClaimsIdentity(accessTokenParameters.Claims);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = accessTokenParameters.Audience,
            Issuer = accessTokenParameters.Issuer,
            NotBefore = notBefore,
            Expires = expires,
            SigningCredentials = signingCredentials,
            Subject = identity,
        };

        var securityTokenHandler = new JwtSecurityTokenHandler();
        var securityToken = securityTokenHandler.CreateToken(securityTokenDescriptor);

        var encodedAccessToken = securityTokenHandler.WriteToken(securityToken);

        return encodedAccessToken;
    }
}

//DEMOSNIPPETS-LABEL 03_Const_Basic_Addition
    public static PemCertificate ValidSigningCertificate { get; } = SelfSignedAccessTokenPemCertificateFactory.Create();
    public static string ValidIssuer { get; } = $"Issuer:Dotnet:WeatherApp:Tests:Project";
    public static string ValidAudience { get; } = $"Audience:Dotnet:WeatherApp:Project";
    public const string SubClaimType = "sub";
    public const string SubClaimValidValue = "sub-value";

//DEMOSNIPPETS-LABEL 04_Program.ReplaceAddJwtBearer
.AddJwtBearer(o =>
{
    o.MapInboundClaims = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        NameClaimType = "sub",
    };
});

//DEMOSNIPPETS-LABEL 05_SignatureValidator
SignatureValidator = (token, parameters) => new JsonWebToken(token)
//DEMOSNIPPETS-LABEL 06_AppsettingsJwtBearerOptions
"Jwt": {
    "Audience": "Audience:Dotnet:WeatherApp:Project",
    "Issuer": "Issuer:Dotnet:WeatherApp:Tests:Project"
}
//DEMOSNIPPETS-LABEL 06_0_PostConfigureJwtBearerOptions

options.ConfigurationManager = ConfigForMockedOpenIdConnectServer.Create();

//DEMOSNIPPETS-LABEL 06_a_CreateOpenIdConfigManager
public class ConfigForMockedOpenIdConnectServer
{
    public static IConfigurationManager<OpenIdConnectConfiguration> Create()
    {
        var openIdHttpClient = new HttpClient(
            new MockingOpenIdProviderMessageHandler(Consts.ValidOpenIdConnectDiscoveryDocumentConfiguration, Consts.ValidSigningCertificate));

        return new ConfigurationManager<OpenIdConnectConfiguration>(
            Consts.WellKnownOpenIdConfiguration, new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(openIdHttpClient));
    }

}

//DEMOSNIPPETS-LABEL 06_b_Mocking
public sealed class MockingOpenIdProviderMessageHandler(
        OpenIdConnectDiscoveryDocumentConfiguration openIdConnectDiscoveryDocumentConfiguration,
        PemCertificate tokenSigningCertificate)
    : HttpMessageHandler
{
    private readonly OpenIdConnectDiscoveryDocumentConfiguration _openIdConnectDiscoveryDocumentConfiguration = openIdConnectDiscoveryDocumentConfiguration ?? throw new ArgumentNullException(nameof(openIdConnectDiscoveryDocumentConfiguration));
    private readonly PemCertificate _tokenSigningCertificate = tokenSigningCertificate ?? throw new ArgumentNullException(nameof(tokenSigningCertificate));

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SendAsync(request, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.RequestUri == null) throw new ArgumentNullException(nameof(request.RequestUri));

        if (request.RequestUri.AbsoluteUri.Contains(Consts.WellKnownOpenIdConfiguration))
            return await GetOpenIdConfigurationHttpResponseMessage();

        if (request.RequestUri.AbsoluteUri.Equals(_openIdConnectDiscoveryDocumentConfiguration.JwksUri))
            return await GetJwksHttpResonseMessage();

        throw new NotSupportedException("I only support mocking jwks.json and openid-configuration");
    }

    private Task<HttpResponseMessage> GetOpenIdConfigurationHttpResponseMessage()
    {
        var httpResponseMessage = new HttpResponseMessage();
        httpResponseMessage.StatusCode = HttpStatusCode.OK;
        httpResponseMessage.Content = JsonContent.Create(_openIdConnectDiscoveryDocumentConfiguration, MediaTypeHeaderValue.Parse("application/json"));
        return Task.FromResult(httpResponseMessage);
    }

    private Task<HttpResponseMessage> GetJwksHttpResonseMessage()
    {
        var httpResponseMessage = new HttpResponseMessage();
        var jwksCertificate = _tokenSigningCertificate.ToJwksCertificate();
        httpResponseMessage.Content = JsonContent.Create(jwksCertificate, MediaTypeHeaderValue.Parse("application/json"));
        httpResponseMessage.StatusCode = HttpStatusCode.OK;
        return Task.FromResult(httpResponseMessage);
    }
}


//DEMOSNIPPETS-LABEL 07_ConstAddition
public static string WellKnownOpenIdConfiguration { get; set; } = "https://i.do.not.exist/.well-known/openid-configuration";
public static OpenIdConnectDiscoveryDocumentConfiguration ValidOpenIdConnectDiscoveryDocumentConfiguration { get; } = OpenIdConnectDiscoveryDocumentConfigurationFactory.Create(Consts.ValidIssuer);

//DEMOSNIPPETS-LABEL 09_AuthorizeBelgium
//.App Weatherforecastcontroller.cs
[Authorize(Policy = "OnlyBelgium")]
//DEMOSNIPPETS-LABEL 10_AuthorizeGetOperation
[Authorize(Policy = "WeatherForecast:Get")]
//DEMOSNIPPETS-LABEL 11_ProgramAddAuthorization
builder.Services.AddAuthorization(authorizationOptions =>
{

    authorizationOptions.AddPolicy("OnlyBelgium", policy =>
    {
        policy.RequireClaim("country", "Belgium");

    });

    authorizationOptions.AddPolicy("WeatherForecast:Get", policy =>
    {
        policy.RequireClaim("scope", "weatherforecast:read");
    });
});
//DEMOSNIPPETS-LABEL 12_ContstsAdditionalData
public const string ScopeClaimType = "scope";
public const string ScopeClaimValidValue = "weatherforecast:read";

public const string CountryClaimType = "country";
public const string CountryClaimValidValue = "Belgium";
//DEMOSNIPPETS-LABEL 13_AddClaimsToToken
,new(Consts.ScopeClaimType, Consts.ScopeClaimValidValue),
new(Consts.CountryClaimType,
    Consts.CountryClaimValidValue)

};
