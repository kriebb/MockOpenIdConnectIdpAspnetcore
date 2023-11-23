//DEMOSNIPPETS-TAB Demo2 Application
//DEMOSNIPPETS-LABEL 01_Test2
//.Tests In Test class
[Fact()]
public async Task WhenWeGetWeatherForecast_WithValidAccessToken_ShouldReturn200()
{
    var accessTokenParameters = new AccessTokenParameters();
    var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
    var response = await httpClient.GetAsync($"/WeatherForecast/");
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
}
//DEMOSNIPPETS-LABEL 02_AccessTokenParameters
//.Tests New file in Test class
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
//DEMOSNIPPETS-LABEL 02a_AccessTokenHandler
//.Tests New File Infrastructure / Jwt
using System.Net.Http.Headers;
using WeatherApp.Demo2.Tests.Controllers;
using Xunit.Abstractions;

namespace WeatherApp.Demo2.Tests.Infrastructure.Jwt;

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

//DEMOSNIPPETS-LABEL 02b_AccessTokenFactory
//.Tests New file in Infrastructure/Jwt
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using WeatherApp.Demo2.Tests.Controllers;

namespace WeatherApp.Demo2.Tests.Infrastructure.Jwt;

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

//DEMOSNIPPETS-LABEL 03_Const_Basic
//.Tests where Testclass is => New file Consts.cs
public class Const
{

    public static PemCertificate ValidSigningCertificate { get; } = SelfSignedAccessTokenPemCertificateFactory.Create();
    public static string ValidIssuer { get; } = $"Issuer:Dotnet:WeatherApp:Tests:Project";
    public static string ValidAudience { get; } = $"Audience:Dotnet:WeatherApp:Project";
    public const string SubClaimType = "sub";
    public const string SubClaimValidValue = "sub-value";

}
//DEMOSNIPPETS-LABEL 04_Program.AddAuthentication
//.App Program.cs
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

//DEMOSNIPPETS-LABEL 06_PostConfigureJwtBearerOptions
//.Tests Serversetupfixture
.ConfigureTestServices(services =>
{

    services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
        options =>
        {
            options.ConfigurationManager = ConfigForMockedOpenIdConnectServer.Create();

//DEMOSNIPPETS-LABEL 06a_CreateOpenIdConfigManager
//.Tests OpenId folder ConfigForMockedOpenIdConnectServer.cs
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

//DEMOSNIPPETS-LABEL 06b_Mocking
//.Tests OpenId folder ConfigForMockedOpenIdConnectServer.cs
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



//DEMOSNIPPETS-LABEL 07_ConstAddition
//.Tests Consts.cs
public static string WellKnownOpenIdConfiguration { get; set; } = "https://i.do.not.exist/.well-known/openid-configuration";
public static OpenIdConnectDiscoveryDocumentConfiguration ValidOpenIdConnectDiscoveryDocumentConfiguration { get; } = OpenIdConnectDiscoveryDocumentConfigurationFactory.Create(Consts.ValidIssuer);

//DEMOSNIPPETS-LABEL 08_DefineConfiguration
//ServerSetupFixture.cs
configuration.AddInMemoryCollection(new[]
{
    new KeyValuePair<string, string?>("Jwt:Issuer", Consts.ValidIssuer),
    new KeyValuePair<string, string?>("Jwt:Audience", Consts.ValidAudience)
});

//DEMOSNIPPETS-LABEL 09_AuthorizeBelgium
//.App Weatherforecastcontroller.cs
[Authorize(Policy = "OnlyBelgium")]
//DEMOSNIPPETS-LABEL 10_AuathorizeGetOperation
//.App Weatherforecastcontroller.cs
[Authorize(Policy = "WeatherForecast:Get")]
//DEMOSNIPPETS-LABEL 11_ProgramAddAuthorization
//.App Program.cs
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
//.Tests Consts.cs
public const string ScopeClaimType = "scope";
public const string ScopeClaimValidValue = "weatherforecast:read";

public const string CountryClaimType = "country";
public const string CountryClaimValidValue = "Belgium";
//DEMOSNIPPETS-LABEL 13_AddClaimsToToken
//.Tests AccessTokenParameters.cs
new(Consts.ScopeClaimType, Consts.ScopeClaimValidValue),
new(Consts.CountryClaimType,
    Consts.CountryClaimValidValue)

};
