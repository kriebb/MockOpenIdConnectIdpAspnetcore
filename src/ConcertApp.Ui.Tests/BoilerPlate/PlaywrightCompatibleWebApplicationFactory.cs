using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
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
            () => TestOutputHelper,
            EnableRecording,
            "https://localhost:3011");
        ConcertsApiDependencyUrl = new Uri(ConcertsApiDependency.Url ?? throw new InvalidOperationException());
    }


    public Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenCode), Token> TokenFactoryFunc { get; set; } 
    public Func<UserInfoEndpointResponseBody> UserInfoResponseFunc { get; set; }

    public Uri ConcertsApiDependencyUrl { get; set; }

    public WireMockServer ConcertsApiDependency { get; set; }
    

    public ITestOutputHelper TestOutputHelper { get; set; } = new ConsoleTestOutputHelper();

    private IHost? _hostThatRunsKestrelImpl;
    private IHost? _hostThatRunsTestServer;

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
                        new("oidc:ClientId","someClientId"),
                        new("oidc:CallbackBaseUri",$"https://localhost:{kestrelPort}/microsoft-signin"),
                        new("ConcertApp:Api",$"https://{ConcertsApiDependencyUrl.Host}:{ConcertsApiDependencyUrl.Port}/"),

                    }!);



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
                    //stubbing microsoft specific library is a bit different
                    services.PostConfigure<MicrosoftAccountOptions>(MicrosoftAccountDefaults.AuthenticationScheme, options =>
                    {
                        var OIDCConfiguration = Consts.ValidOpenIdConnectDiscoveryDocumentConfiguration(Constants.ValidIssuer);
                        var backChannelMessageHandler = ConfigForMockedOpenIdConnectServer.CreateHttpHandler(Constants.ValidIssuer, TokenFactoryFunc, UserInfoResponseFunc);
                        var backChannelHttpClient = ConfigForMockedOpenIdConnectServer.CreateHttpClient(backChannelMessageHandler);
                        
                        
                        options.TokenEndpoint = OIDCConfiguration.TokenEndpoint;
                        options.AuthorizationEndpoint = OIDCConfiguration.AuthorizationEndpoint;
                        options.UserInformationEndpoint = OIDCConfiguration.UserinfoEndpoint;
                        //backchannel(httphandler) is the httpclient(messagehandler) that is used to make the request to the identity provider when trading the authorization code for the token
                        options.Backchannel = backChannelHttpClient;
                        options.Events = new OAuthEvents()
                        {
                            OnAccessDenied = context =>
                            {
                                var logger = Services.GetRequiredService<ILogger<PlaywrightCompatibleWebApplicationFactory>>();
                                
                                logger?.LogInformation("On Access Denied. Result: {0} Failure:{1}", context.Result.Succeeded, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnCreatingTicket = context => {
                                var logger = Services.GetRequiredService<ILogger<PlaywrightCompatibleWebApplicationFactory>>();

                                logger?.LogInformation("On Creating Ticket. Result: {0} Failure:{1}",context.Result?.Succeeded, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnRemoteFailure = context =>
                            {
                                var logger = Services.GetRequiredService<ILogger<PlaywrightCompatibleWebApplicationFactory>>();
                                
                                logger?.LogInformation("On Remote Failure. Result: {0} Failure:{1}", context.Result?.Succeeded, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnTicketReceived = context =>
                            {
                                var logger = Services.GetRequiredService<ILogger<PlaywrightCompatibleWebApplicationFactory>>();

                                logger?.LogInformation("On Ticket Received. User: {0} Claims:{1}", context.Principal?.Identity?.Name, 
                                    string.Join(",", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
                                return Task.CompletedTask;
                            },
                            OnRedirectToAuthorizationEndpoint = context => {
                                var logger = Services.GetRequiredService<ILogger<PlaywrightCompatibleWebApplicationFactory>>();

                                //redirect to the authorization endpoint in the browser. This is the endpoint where the user will authenticate
                                //context.Response.Redirect(context.RedirectUri); //redirect because when  you reassign, redirect is not happening
                                //but we need to override it, so we can do a mock redirect from the IDP to the server. From there, the server will do
                                //a backchannel request to the IDP to get the token
                                
                                var authorizationCodeInHeader = backChannelMessageHandler.GetAuthorizationLocationHeader(context.RedirectUri);
                                context.Response.Redirect(authorizationCodeInHeader);
                                
                                logger?.LogInformation("Redirected to authorization endpoint:" + context.RedirectUri);
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