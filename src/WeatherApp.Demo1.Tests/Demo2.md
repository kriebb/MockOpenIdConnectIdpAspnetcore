1. Write the test for a valid accessToken
Explain what the test does:

- we will create default valid accesstokenparameters class
- we will add the accesstoken to the header of the message using the JwtBearerCustomAccessTokenHandler

WeatherForecastControllerTests
```
       [Fact()]
        public async Task WhenWeGetWeatherForecast_WithValidAccessToken_ShouldReturn200()
        {
           var accessTokenParameters = new AccessTokenParameters();


            var httpClient = _fixture.CreateDefaultClient(new JwtBearerCustomAccessTokenHandler(accessTokenParameters, _testOutputHelper));
            var response = await httpClient.GetAsync($"/WeatherForecast/");

         
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
```

2. Copy/Paste AccessToken Parameters in the test class and Discuss what parameters are needed for a valid accesstoken:

- Signing Certificate => valid Certificate
- Audience: for who is the token
- Issuer: who gives out the token
- What does the token of data (sub) = id

``` 
public record AccessTokenParameters
{
    public X509Certificate2 SigningCertificate { get; set; } = Consts.ValidSigningCertificate.ToX509Certificate2();
    public string Audience { get; set; } = Consts.ValidAudience;
    public string Issuer { get; set; } = Consts.ValidIssuer;
    public List<Claim> Claims { get; set ; } = new()
    {
        new(Consts.SubClaimType, Consts.SubClaimValidValue)

    };
}
```

2. I gather the data in the const class. Lets make sure that the AccessTokenParamaters are filled with correct data.

- Note that there is a Selfsigned Pem Certificate Factory ready to use. => Infrastructure / Security
```
public class Const
{

    public static PemCertificate ValidSigningCertificate { get;  }= SelfSignedAccessTokenPemCertificateFactory.Create();
    public static string ValidIssuer { get; } = $"Issuer:Dotnet:WeatherApp:Tests:Project";
    public static string ValidAudience { get; }= $"Audience:Dotnet:WeatherApp:Project";
    public const string SubClaimType = "sub";
    public const string SubClaimValidValue = "sub-value";

}
```

- Let us have a peek into the selfsigned accesstoken pem certificate

3. We have all the data to create an accesstoken. Lets add it to the request.
```
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
```

- You can see the JwtBearerAccessTokenFactory that will create the bearer token, using the accesstokenparameters
```
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
```

- Run the test.

- What the output of the test

```
Generated the following encoded accesstoken
Authentication Failed. Result: IDX10204: Unable to validate issuer. validationParameters.ValidIssuer 
```

Run the first test => No header and still works.
We miss configuration at the server. Lets configure our application.

=> Move to Demo2.md of WeatherApp.

/////////////////////////////////////////////////////////////////////////////////////////

=> Come back after the demo. Situation is that the program.cs is adjusted
                        
We need to specify the public key so the signature can be validated.

In the ServerSetupFixte 

```
  .ConfigureTestServices(services =>
            {

                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.ConfigurationManager = ConfigForMockedOpenIdConnectServer.Create();

```

Go into the ConfigForMockedOpenIdConnectServer class. Discuss 
=> Will generate the OpenidConfiguration when it is asked
Using the classMockingOpenIdProviderMessageHandler

Const class
- Define the url where the openidconfiguration can be fetched
- Define the object that should be returned when the url is called
```
    public static string WellKnownOpenIdConfiguration { get; set; } = "https://i.do.not.exist/.well-known/openid-configuration";
    public static OpenIdConnectDiscoveryDocumentConfiguration ValidOpenIdConnectDiscoveryDocumentConfiguration { get; } = OpenIdConnectDiscoveryDocumentConfigurationFactory.Create(Consts.ValidIssuer);
    

```
Go into the OpenIdConnectDiscoveryDocumentConfigurationFactory.Create method. => It is there thet the object is returned but ValidCertificate is converted to JWKS when the JWKS endpoint is fetched.


=> After configuraiton is done: Run the test again.
= Fails! On the production is not yet an issuer/audience specified!. Lets specify that in the ServerSetupFixture

Authentication Failed. Result: IDX10208: Unable to validate audience. 

=>  Add configuren in the serversetupfixture
```

                configuration.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("Jwt:Issuer", Consts.ValidIssuer),
                    new KeyValuePair<string, string?>("Jwt:Audience", Consts.ValidAudience)
                });
```

==> Succeeds!



And now we can play

=> We are going to add validation on data in the claim. There should be 2 claims in the token:

- the country and the scope
- 

- country: the service should only be called from Belgium
- scope: you need to have access to the get-operation

=> Move to Demo2b.md of WeatherApp.

/////////////////////////////////////////////////////////////////////////////////////////