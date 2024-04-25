using System.Security.Cryptography;
using System.Text;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using OIdcMockingInfrastructure.Jwt;
using OIdcMockingInfrastructure.Models;

namespace ConcertApp.Ui.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture()]
public class GivenHomePage : PageTest
{

    public GivenHomePage()
    {
        
    }
        
    public override BrowserNewContextOptions ContextOptions()
    {
        BrowserNewContextOptions? options = new()
            {
                IgnoreHTTPSErrors = true,
                RecordHarMode = HarMode.Full,
                RecordHarContent = HarContentPolicy.Embed,
                RecordHarPath = @"c:\tools\har.har",
            };
        

        return options;
    }


    [SetUp]
    public async Task Setup()
    {
         
        Page.Request += (_, request) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine(">> " + request.Method + " " + request.Url + System.Environment.NewLine + Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>()));
        Page.RequestFailed += (_, request) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine(">> RequestFailed: " + request.Method + " " + request.Url + System.Environment.NewLine + Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>()));
        Page.RequestFinished += (_, request) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine(">> RequestFinished: " + request.Method + " " + request.Url + System.Environment.NewLine + Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>()));

        Page.Response += (_, response) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine("<< " + response.Status + " " + response.Url + System.Environment.NewLine);
        

        Page.PageError += (_, error) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine("<< " + error);
        Page.Crash += (_, error) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine("<< " + error);

        Page.SetDefaultTimeout(3000);
        Page.SetDefaultNavigationTimeout(3000);

        SetUpConfig.WebAppFactory.TokenFactoryFunc = args =>
        {
            var scope = args.AuthorizationCodeRequestQuery["scope"]!;
            var nonce = args.AuthorizationCodeRequestQuery["nonce"];
            var userId = Constants.UserId;

            var token = new Token(
                AccessToken: CreateAccessToken(userId, scope),
                RefreshToken: CreateRefreshToken(),
                IdToken: CreateIdToken(userId, nonce, scope));

            return token;
        };
        
        SetUpConfig.WebAppFactory.UserInfoResponseFunc = () => new UserInfoEndpointResponseBody(
            ODataContext: Constants.ODataContext,
            BusinessPhones: Constants.BusinessPhones,
            DisplayName: Constants.DisplayName,
            GivenName: Constants.GivenName,
            JobTitle: Constants.JobTitle,
            Mail: Constants.Mail,
            MobilePhone: Constants.MobilePhone,
            OfficeLocation: Constants.OfficeLocation,
            PreferredLanguage: Constants.PreferredLanguage,
            Surname: Constants.Surname,
            UserPrincipalName: Constants.UserPrincipalName,
            Id: Constants.UserId
        );
        SetUpConfig.WebAppFactory.ConcertsApiDependency.ResetMappings();
        await Page.GotoAsync($"{SetUpConfig.WebAppFactory.ServerAddress}");

    }


    [Test]
    public async Task WhenWeOpenTheHomepage_TheWelcomeTextShouldAppear()
    {
        Page.SetDefaultTimeout(30000);
        Page.SetDefaultNavigationTimeout(30000);

        await Page.GotoAsync($"{SetUpConfig.WebAppFactory.ServerAddress}");

        await Expect(Page.GetByText( "Welcome to ConcertApp!")).ToBeInViewportAsync();
    }
    
    [Test]
    public async Task WhenWeClickOnLogin_WeShouldSeeATextWelcomingTheUser()
    {
        Page.SetDefaultTimeout(30000);
        Page.SetDefaultNavigationTimeout(30000);

        await Page.GotoAsync($"{SetUpConfig.WebAppFactory.ServerAddress}");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(Page.GetByText( "Welcome Kristof Riebbels!")).ToBeInViewportAsync();
    }
    
    
    public string CreateAccessToken(string userid, string scope)
    {
        var accessToken = JwtTokenFactory.Create(
            new AccessTokenParameters(
                Constants.ValidAudience, 
                Constants.ValidIssuer, 
                userid, 
                scope,
                Constants.ValidCountryClaimValue));
        return accessToken;
    }

    public string CreateIdToken(string userId, string? nonce, string scope)
    {
        var idTokenParam = new IdTokenParameters(
            userId,
            nonce,
            scope,
            Constants.ValidAudience,
            Constants.ValidIssuer,
            Constants.ValidCountryClaimValue);


        idTokenParam
            .AddOrReplaceClaim("uid", "uid_" + userId)
            .AddOrReplaceClaim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

        var idToken = JwtTokenFactory.Create(idTokenParam);
        return idToken;
    }

    public string CreateRefreshToken()
    {
        // This is a simple method to create a secure random string for a refresh token.
        // In a production system, you may need to include additional logic to manage issuance,
        // storage, revocation, and security of refresh tokens.
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(randomBytes);

        return refreshToken;
    }
}