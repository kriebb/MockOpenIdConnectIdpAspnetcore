using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ConcertApp.Ui.Tests.BoilerPlate;
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
                RecordVideoDir = @"c:\tools\video"
            };
        
        return options;
    }

    public ILoggerFactory? LoggerFactory { get; set; }
    [SetUp]
    public async Task Setup()
    {
        bool ShouldIgnore(string url)
        {
            // Matches .js or .css followed by an optional version query string ?v=
            var pattern = @".*\.(js|css)(\?v=.*)?$";
            var isResource = Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);

            // Matches the root URL of the application, regardless of the port number
            var isRoot = false;//new Uri(url).AbsolutePath == "/";

            return isResource || isRoot;
        }

        //Ensure that logging happens in the right output window
        LoggerFactory = PlaywrightCompatibleWebApplicationFactory.CreateLoggerFactory();
        SetUpConfig.WebAppFactory!.ResourceServerOidc = LoggerFactory.CreateLogger("ResourceServer-Oidc");
        Page.Request += (_, request) =>
        {
            if (!ShouldIgnore(request.Url))
            {


                var logger = LoggerFactory.CreateLogger("Browser.Request");


                var eventInfo = IncrementEventCounter(request.Url);
                var logMessage = FormatLogMessage(
                    eventInfo,
                    "Request Sent",
                    $"Method: {request.Method}",
                    $"Url: {request.Url}",
                    $"PostData: {Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>())}",
                    $"Headers: {BuildHeadersString(request.Headers)}"
                );
                logger.LogInformation(logMessage);
            }
        };


        Page.RequestFailed += (_, request) =>
        {
            if (!ShouldIgnore(request.Url))
            {
                var logger = LoggerFactory.CreateLogger("Browser.RequestFailed");


                var eventInfo = IncrementEventCounter(request.Url);
                var logMessage = FormatLogMessage(
                    eventInfo,
                    "Request Failed",
                    $"Method: {request.Method}",
                    $"Url: {request.Url}",
                    $"PostData: {Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>())}",
                    $"Headers: {BuildHeadersString(request.Headers)}"
                );
                logger.LogError(logMessage);
            }
        };

        Page.RequestFinished += (_, request) =>
        {
            // do nothing. not needed for this demo
        };


        Page.Response += (_, response) =>
        {
            if (!ShouldIgnore(response.Url) || response.Status != 200)
            {
                var logger = LoggerFactory.CreateLogger("Browser.Response");


                {

                    var eventInfo = IncrementEventCounter(response.Request.Url);
                    var logMessage = FormatLogMessage(
                        eventInfo,
                        "Response Received",
                        $"Status: {response.Status}",
                        $"Url: {response.Url}",
                        $"Headers: {BuildHeadersString(response.Headers)}"
                    );
                    logger.LogInformation(logMessage);

                }
            }
        };


        Page.SetDefaultTimeout(3000);
        Page.SetDefaultNavigationTimeout(3000);

        SetUpConfig.WebAppFactory!.TokenFactoryFunc = args =>
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
            DisplayName: Constants.DisplayName,
            GivenName: Constants.GivenName,
            Mail: Constants.Mail,
            MobilePhone: Constants.MobilePhone,
            PreferredLanguage: Constants.PreferredLanguage,
            Surname: Constants.Surname,
            UserPrincipalName: Constants.UserPrincipalName,
            Id: Constants.UserId
        );
        
        SetUpConfig.WebAppFactory.ConcertsApiDependency.ResetMappings();

        LoggerFactory.CreateLogger("SetUp").LogInformation($"Navigating to {SetUpConfig.WebAppFactory.ServerAddress}. Ignoring  logs that have only this address, or requests for javascript and css files, for demo purposes.");
        await Page.GotoAsync($"{SetUpConfig.WebAppFactory.ServerAddress}");

    }


    [Test]
    public async Task WhenWeOpenTheHomepage_TheWelcomeTextShouldAppear()
    {
        
        var logger = LoggerFactory!.CreateLogger(nameof(WhenWeOpenTheHomepage_TheWelcomeTextShouldAppear));
        logger.LogInformation("Starting test");
        
        Page.SetDefaultTimeout(30000);
        Page.SetDefaultNavigationTimeout(30000);

        await Page.GotoAsync($"{SetUpConfig.WebAppFactory!.ServerAddress}");

        await Expect(Page.GetByText( "Welcome to ConcertApp!")).ToBeInViewportAsync();
    }
    
    [Test]
    public async Task WhenWeClickOnLogin_WeShouldSeeATextWelcomingTheUser()
    {
        var logger = LoggerFactory!.CreateLogger(nameof(WhenWeClickOnLogin_WeShouldSeeATextWelcomingTheUser));
        logger.LogInformation("Starting test");
        
        Page.SetDefaultTimeout(30000);
        Page.SetDefaultNavigationTimeout(30000);

        await Page.GotoAsync($"{SetUpConfig.WebAppFactory!.ServerAddress}");
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

    private int _eventCounter = 0;
    private readonly ConcurrentDictionary<string, int> _sequenceNumbers = new();

    private string BuildHeadersString(Dictionary<string, string>? headers)
    {
        StringBuilder headersBuilder = new StringBuilder();
        if (headers != null)
        {
            foreach (var header in headers)
            {
                // Ignore specified headers
                if (header.Key.ToLower() != "user-agent" &&
                    header.Key.ToLower() != "sec-ch-ua" &&
                    header.Key.ToLower() != "sec-ch-ua-mobile" &&
                    header.Key.ToLower() != "sec-ch-ua-platform" &&
                    header.Key.ToLower() != "expires" &&
                    header.Key.ToLower() != "samesite" &&
                    header.Key.ToLower() != "server" &&
                    header.Key.ToLower() != "date")
                {
                    headersBuilder.AppendLine($"{header.Key}: {header.Value}");
                }
            }
        }

        return headersBuilder.ToString();
    }

    private (int EventCounter, int SequenceNumber) IncrementEventCounter(string traceIdentifier)
    {
        int eventCounter;
        int sequenceNumber;

        lock (_sequenceNumbers)
        {
            eventCounter = ++_eventCounter;
            sequenceNumber = _sequenceNumbers.AddOrUpdate(traceIdentifier, 1, (_, value) => value + 1);
        }

        return (eventCounter, sequenceNumber);
    }

    private string FormatLogMessage((int EventCounter, int SequenceNumber) eventInfo, string eventName, params string[] details)
    {
        const int maxLineLength = 80;
        const string indent = "    ";

        var builder = new StringBuilder();
        builder.AppendLine($"Event {eventInfo.EventCounter}.{eventInfo.SequenceNumber}: {eventName}");

        foreach (var detail in details)
        {
            var words = detail.Split(';');

            var line = indent;
            foreach (var word in words)
            {
                if ((line + word).Length > maxLineLength)
                {
                    builder.AppendLine(line);
                    line = indent;
                }

                line += $"{word}";
            }

            builder.AppendLine(line);
        }

        return builder.ToString();
    }

    [TearDown]
    public void Dispose()
    {
        LoggerFactory?.Dispose();
    }



}