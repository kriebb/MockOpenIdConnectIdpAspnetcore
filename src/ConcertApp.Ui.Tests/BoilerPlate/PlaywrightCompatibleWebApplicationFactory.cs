using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NUnit.Framework;
using OIdcMockingInfrastructure;
using OIdcMockingInfrastructure.Models;
using OIdcMockingInfrastructure.OpenId;
using WireMock.Server;

namespace ConcertApp.Ui.Tests.BoilerPlate;

public class PlaywrightCompatibleWebApplicationFactory :  WebApplicationFactory<Program>
{

    private const bool EnableRecording = false;
    private const string DevelopmentEnvironment = "Development";
    private const string Environment = DevelopmentEnvironment;


    public PlaywrightCompatibleWebApplicationFactory()
    {
        var wireMockServerFactory = new WireMockServerFactory();
    

        ConcertsApiDependency = wireMockServerFactory.CreateDependency(
            () => Services.GetRequiredService<ILogger<WireMockServerFactory>>(),
            EnableRecording,
            "https://localhost:3011");
        ConcertsApiDependencyUrl = new Uri(ConcertsApiDependency.Url ?? throw new InvalidOperationException());
    }


    public Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenCode), Token> TokenFactoryFunc { get; set; } 
    public Func<UserInfoEndpointResponseBody> UserInfoResponseFunc { get; set; }

    public Uri ConcertsApiDependencyUrl { get; set; }

    public WireMockServer ConcertsApiDependency { get; set; }
    
    
    private IHost? _hostThatRunsKestrelImpl;
    private IHost? _hostThatRunsTestServer;

    private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);
    private ILoggerFactory? _logger;

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
    
    public ILoggerFactory LoggerFactory => _logger ??= Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder
        .AddConsole()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Trace));

    /// <summary>
    /// CreateHost to ensure we can use the deferred way of capturing the program.cs Webhostbuilder without refactoring program.cs
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var logger = LoggerFactory.CreateLogger<PlaywrightCompatibleWebApplicationFactory>();
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
                    options.ListenLocalhost(kestrelPort, x => x.UseHttps());
                }))
                .ConfigureAppConfiguration(
                    configuration =>
                    {

                        configuration.AddJsonFile("appsettings.json");
                        configuration.AddJsonFile($"appsettings.{Environment}.json");
                        configuration.AddInMemoryCollection(new List<KeyValuePair<string, string>>()
                        {

                            new("Authentication:Microsoft:ClientId", "fakeClientId"),
                            new("Authentication:Microsoft:ClientSecret", "f4k3Cl13ntS3cr3t"),
                            new("Authentication:Microsoft:ValidIssuer", Constants.ValidIssuer),
                            new("Authentication:Microsoft:ValidAudience", Constants.ValidAudience),
                            new("Authentication:Microsoft:Authority", Constants.ValidAuthority),

                            new("ConcertApp:Api", $"https://{ConcertsApiDependencyUrl.Host}:{ConcertsApiDependencyUrl.Port}/"),

                        }!);



                        configuration.AddUserSecrets<Program>();

                    })
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                })
                .ConfigureServices(services =>
                {
                    
                    services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme,
                    options =>
                    {
                        var backChannelMessageHandler = ConfigForMockedOpenIdConnectServer.CreateHttpHandler(Constants.ValidIssuer, TokenFactoryFunc, UserInfoResponseFunc);
                        options.ConfigurationManager = ConfigForMockedOpenIdConnectServer.Create(backChannelMessageHandler);
                        options.Backchannel =
                            ConfigForMockedOpenIdConnectServer.CreateHttpClient(backChannelMessageHandler); //needed to fetch the access_token and id_token
                        options.Events = new OpenIdConnectEvents()
                        {
                            OnAuthenticationFailed = context =>
                            {
                                
                                logger.LogError("Authentication Failed. Result: {0} Failure:{1}", context.Exception.Message, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnAccessDenied = context => {

                                logger.LogError("Access Denied. Result: {0} Failure:{1}",context.Result?.Succeeded, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {

                                logger.LogInformation("Token Validated. User: {0} Claims:{1}", context.Principal?.Identity?.Name, 
                                    string.Join(",", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
                                return Task.CompletedTask;
                            },
                            OnMessageReceived = context =>
                            {
                                logger.LogInformation($"Message Received: Token?: [{context.Token}]" );

                                return Task.CompletedTask;
                            },
                            OnRedirectToIdentityProvider = context =>
                            {
                                //Middleware for Browser! Does not pass trough the ConfigurationManager
                                //This happens after this event. Why I do not know ==> TODO: ask on GITHUB
                                context.Properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, context.ProtocolMessage?.RedirectUri);
                                context.ProtocolMessage!.State = context.Options.StateDataFormat.Protect(context.Properties);
                                
                                var authorizationRequestUri = context.ProtocolMessage?.BuildRedirectUrl()!;
                                var mockedAuthorizationCode = backChannelMessageHandler.GetAuthorizationLocationHeaderFromFullUri(authorizationRequestUri);
                                
                                logger?.LogInformation("Override Browser Redirect! Redirected to authorization endpoint:" + mockedAuthorizationCode);

                                context.HandleResponse();
                                context.Response.Redirect(mockedAuthorizationCode);
                                
                                return Task.CompletedTask;
                            },
                            OnRedirectToIdentityProviderForSignOut = context =>
                            {

                                logger.LogInformation("Redirect to Identity Provider for SignOut. RedirectUri: {0}", context.ProtocolMessage?.RedirectUri);
                                return Task.CompletedTask;
                            },
                            OnRemoteFailure = context =>
                            {

                                logger.LogError("Remote Failure. Result: {0} Failure:{1}", context.Result?.Succeeded, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnSignedOutCallbackRedirect = context =>
                            {

                                logger.LogInformation("Signed Out Callback Redirect. RedirectUri: {0}", context.ProtocolMessage?.RedirectUri);
                                return Task.CompletedTask;
                            },
                            OnRemoteSignOut = context =>
                            {

                                logger.LogInformation("Signed Out Redirect to Identity Provider. RedirectUri: {0}", context.ProtocolMessage?.RedirectUri);
                                return Task.CompletedTask;
                            },
                            OnTokenResponseReceived = context =>
                            {
                                logger.LogInformation("Token Response Received. Token: {0}", context.TokenEndpointResponse.AccessToken);
                                return Task.CompletedTask;
                            },
                            OnTicketReceived = context =>
                                {
                                    logger.LogInformation("Ticket Received. ReturnUri: {0}", context.ReturnUri);
                                    return Task.CompletedTask;
                                },
                            OnAuthorizationCodeReceived = context =>
                            {
                                logger.LogInformation("Authorization Code Received. Code: {0}", context.ProtocolMessage.Code);
                                
                                return Task.CompletedTask;
                            },
                            OnUserInformationReceived = context =>
                            {
                                logger.LogInformation("UserInformation Received. User: {0}", context.User.RootElement.ToString() );
                                return Task.CompletedTask;
                            }
                        };
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

