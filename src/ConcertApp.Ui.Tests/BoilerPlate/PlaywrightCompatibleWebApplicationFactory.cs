using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using ConcertApp.Ui;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using WeatherApp.Ui.Tests.Assets;
using WeatherApp.Ui.Tests.BoilerPlate.Json;
using WireMock.RequestBuilders;
using WireMock.Server;
using WireMock.Types;
using WireMock.Util;

namespace WeatherApp.Ui.Tests.BoilerPlate;

public class PlaywrightCompatibleWebApplicationFactory :  WebApplicationFactory<Program>
{

    private const bool EnableRecording = false;
    private const string DevelopmentEnvironment = "Development";
    private const string Environment = DevelopmentEnvironment;


    public PlaywrightCompatibleWebApplicationFactory()
    {
        var wireMockServerFactory = new WireMockServerFactory();
        OidcDependency = wireMockServerFactory.CreateDependency(
            () => TestOutputHelper,
            EnableRecording,
            "https://login.microsoft.com");
        OidcDependencyUrl = new Uri(OidcDependency.Url ?? throw new InvalidOperationException());


        OidcDependency.Given(Request.Create().UsingGet().WithPath(x => x.Contains("/.well-known/openid-configuration")))
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithBody(EmbeddedResourceReader.ReadAsset("Assets.oidc-openid-config.json")
                    .Replace("https://login.microsoft.com/", $"https://{OidcDependencyUrl.Host}:{OidcDependencyUrl.Port}/"))
                .WithHeader("content-type", "application/json"));
        OidcDependency.Given(Request.Create().UsingGet().WithPath(x => x.Contains("/pf/JWKS")))
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithBody(EmbeddedResourceReader.ReadAsset("Assets.oidc-wellknown-keys.json"))
                .WithHeader("content-type", "application/json"));
        
        
        OidcDependency.Given(Request.Create().UsingGet().WithPath(x => x.Contains("authorization.oauth2"))).RespondWith(
            WireMock.ResponseBuilders.Response.Create()
                .WithCallback(request =>
                {
                    // Extracting query parameters from the actual request
                    var client_id = request.Query["client_id"].ToString();
                    var redirect_uri = request.Query["redirect_uri"].ToString();
                    var response_type = request.Query["response_type"].ToString();
                    var scope = request.Query["scope"].ToString();
                    var state = request.Query["state"].ToString();
                    _nonce = request.Query["nonce"].ToString();
                    // Assuming the captured redirect_uri is already URL-encoded (as it should be)
                    // Build the Location header with the captured redirect_uri
                    string locationHeader = Uri.UnescapeDataString(redirect_uri);
                    locationHeader += $"?code=1234567890&state={state}";

                    // Provide the response with the redirection status and headers
                    var message = new WireMock.ResponseMessage();
                    message.StatusCode = 302; // HTTP status code for redirection
                    message.Headers = new Dictionary<string, WireMockList<string>>()
                        { { "Location", locationHeader } }; // Redirect to the captured URI

                    return message;
                }));
        OidcDependency.Given(Request.Create().UsingPost().WithPath(x => x.Contains("token.oauth2"))).RespondWith(
            WireMock.ResponseBuilders.Response.Create()
                .WithCallback(request =>
                {
                    var message = new WireMock.ResponseMessage();
                    message.StatusCode = 200; // HTTP status code for redirection
                    message.Headers = new Dictionary<string, WireMockList<string>>()
                        { { "Content-Type", "application/json" } }; // Redirect to the captured URI

                    message.BodyDestination = null;
                    message.BodyData = new BodyData
                    {
                        Encoding = Encoding.UTF8,
                        DetectedBodyType = BodyType.Json,
                        BodyAsJson = new
                        {
                            access_token = CreateAccessToken("TEST001", "openid profile").AccessToken,
                            token_type = "Bearer",
                            expires_in = 3600,
                            refresh_token = CreateRefreshToken().RefreshToken,
                            id_token = CreateIdToken("TEST001",
                                _nonce ?? throw new NullReferenceException("Nonce was null")).IDToken,
                            scope = "openid profile"
                        },
                        BodyAsJsonIndented = true
                    };


                    return message;
                }));

        ConcertsApiDependency = wireMockServerFactory.CreateDependency(
            () => TestOutputHelper,
            EnableRecording,
            "https://localhost:3011");
        ConcertsApiDependencyUrl = new Uri(ConcertsApiDependency.Url ?? throw new InvalidOperationException());
    }

    public Uri OidcDependencyUrl { get; set; }

    public Uri ConcertsApiDependencyUrl { get; set; }

    public WireMockServer ConcertsApiDependency { get; set; }


    public WireMockServer OidcDependency { get; }


    public ITestOutputHelper TestOutputHelper { get; set; } = new ConsoleTestOutputHelper();

    private IHost? _hostThatRunsKestrelImpl;
    private IHost? _hostThatRunsTestServer;

    private string _nonce;
    private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);

    public static int GetAvailablePort()
    {
        var port = -1;
        while (port == -1)
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(DefaultLoopbackEndpoint);
            if (socket.LocalEndPoint != null) port = ((IPEndPoint)socket.LocalEndPoint).Port;
        }

        return port;
    }
    /// <summary>
    /// CreateHost to ensure we can use the deferred way of capturing the program.cs Webhostbuilder without refactoring program.cs
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var kestrelPort = GetAvailablePort();
        try
        {

            // Create the host for TestServer now before we  
            // modify the builder to use Kestrel instead.    
            _hostThatRunsTestServer = builder.Build();

            // Modify the host builder to use Kestrel instead  
            // of TestServer so we can listen on a real address.    

            builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel(options =>
                {
                    options.ListenLocalhost(kestrelPort, x=> x.UseHttps());
                }))
                   .ConfigureAppConfiguration(
                configuration =>
                {

                    configuration.AddJsonFile("appsettings.json");
                    configuration.AddJsonFile($"appsettings.{Environment}.json");
                    configuration.AddInMemoryCollection(new List<KeyValuePair<string, string>>()
                    {

                        new("oidc:Prompt","login"),
                        new("oidc:Authority",$"https://{OidcDependencyUrl.Host}:{OidcDependencyUrl.Port}/"),
                        new("oidc:ClientId","someClientId"),
                        new("oidc:CallbackBaseUri",$"https://localhost:{kestrelPort}/microsoft-signin"),
                        new("ConcertApp:Api",$"https://{ConcertsApiDependencyUrl.Host}:{ConcertsApiDependencyUrl.Port}/"),

                    });



                    configuration.AddUserSecrets<Program>();

                })
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddProvider(
                    new NUnitLoggerProvider(() => TestOutputHelper ?? new ConsoleTestOutputHelper()));
            })
            .ConfigureServices(services =>
            {
                services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = TestAuthorisationConstants.Audience,
                        ValidIssuer = TestAuthorisationConstants.Issuer
                    };
                    //options.Audience = TestAuthorisationConstants.Audience;
                    options.Authority = TestAuthorisationConstants.Issuer;
                    options.MetadataAddress = new Uri(OidcDependency.Url!) + ".well-known/openid-configuration";
                });
            });
            // Create and start the Kestrel server before the test server,  
            // otherwise due to the way the deferred host builder works    
            // for minimal hosting, the server will not get "initialized    
            // enough" for the address it is listening on to be available.    
            // See https://github.com/dotnet/aspnetcore/issues/33846.    

            _hostThatRunsKestrelImpl = builder.Build();
            _hostThatRunsKestrelImpl.Start();

            // Extract the selected dynamic port out of the Kestrel server  
            // and assign it onto the client options for convenience so it    
            // "just works" as otherwise it'll be the default http://localhost    
            // URL, which won't route to the Kestrel-hosted HTTP server.     

            var server = _hostThatRunsKestrelImpl.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            ClientOptions.BaseAddress = addresses!.Addresses
                .Select(x => new Uri(x))
                .Last();

            // Return the host that uses TestServer, rather than the real one.  
            // Otherwise the internals will complain about the host's server    
            // not being an instance of the concrete type TestServer.    
            // See https://github.com/dotnet/aspnetcore/pull/34702.   

            _hostThatRunsTestServer.Start();
            return _hostThatRunsTestServer;

        }
        catch (Exception)
        {
            _hostThatRunsKestrelImpl?.Dispose();
            _hostThatRunsTestServer?.Dispose();
            throw;
        }
    }

    public string ServerAddress
    {
        get
        {
            EnsureServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }

    public override IServiceProvider Services
    {
        get
        {
            EnsureServer();
            return _hostThatRunsKestrelImpl?.Services ?? throw new ArgumentNullException(nameof(_hostThatRunsKestrelImpl.Services));
        }
    }


    private void EnsureServer()
    {
        if (_hostThatRunsKestrelImpl is null)
        {
            // This forces WebApplicationFactory to bootstrap the server  
            using var _ = CreateDefaultClient();
        }
    }


    public Token CreateAccessToken(string userId, string scope)
    {
        var token = new JwtTokenBuilder()
            .ForSubject(userId)
            .ForAudience(TestAuthorisationConstants.Audience)
            .IssuedBy(TestAuthorisationConstants.Issuer)
            .WithClaim("scope", scope)
            .WithClaim("uid", "uid_"+userId)
            .WithClaim("member_of", JsonConvert.SerializeObject(new []{"test 1", "test 2","Customer User Admin" }))

            .WithSigningCertificate(
                EmbeddedResourceReader.GetCertificate("openssl_crt.pem", "private-key-traditional.pem"));


        return new Token { AccessToken = token.BuildToken() };
    }

    public Token CreateIdToken(string userId, string nonce)
    {


        var token = new JwtTokenBuilder()
            .ForSubject(userId)
            .ForAudience(TestAuthorisationConstants.Audience)
            .IssuedBy(TestAuthorisationConstants.Issuer)
            .WithClaim("nonce", nonce)
            .WithClaim("uid", "uid_"+userId)

            .WithClaim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            // Include other necessary OpenID Connect claims
            .WithSigningCertificate(
                EmbeddedResourceReader.GetCertificate("openssl_crt.pem", "private-key-traditional.pem"));

        return new Token { IDToken = token.BuildToken() };
    }

    public Token CreateRefreshToken()
    {
        // This is a simple method to create a secure random string for a refresh token.
        // In a production system, you may need to include additional logic to manage issuance,
        // storage, revocation, and security of refresh tokens.
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(randomBytes);

        return new Token { RefreshToken = refreshToken };
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        try
        {
            _hostThatRunsTestServer?.Dispose();

            if (_hostThatRunsTestServer != null)
            {
                await _hostThatRunsTestServer.StopAsync().ConfigureAwait(false);
                _hostThatRunsTestServer?.Dispose();
            }
        }
        catch (Exception)
        {
        }

        try
        {
            _hostThatRunsKestrelImpl?.Dispose();

            if (_hostThatRunsKestrelImpl != null)
            {
                await _hostThatRunsKestrelImpl.StopAsync().ConfigureAwait(false);
                _hostThatRunsKestrelImpl?.Dispose();
            }
        }
        catch (Exception)
        {
        }


    }
}