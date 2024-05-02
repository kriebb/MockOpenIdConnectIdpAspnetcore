using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ConcertApp.Ui.Tests.BoilerPlate.Json;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Console;
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


    public Func<(NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenCode), Token>? TokenFactoryFunc { get; set; } 
    public Func<UserInfoEndpointResponseBody>? UserInfoResponseFunc { get; set; }

    public Uri ConcertsApiDependencyUrl { get; set; }

    public WireMockServer ConcertsApiDependency { get; set; }
    
    
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

    public static ILoggerFactory CreateLoggerFactory()
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder
            .AddConsole(x => x.FormatterName = "custom")
            .SetMinimumLevel(LogLevel.Trace).Services.AddSingleton<ConsoleFormatter, CustomConsoleFormatter>()
        );

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
                    loggingBuilder.AddConsole(x => x.FormatterName = "custom");

                    loggingBuilder.AddDebug();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ConsoleFormatter, CustomConsoleFormatter>();

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
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Authentication Failed",
                                    $"Scheme: {context.Scheme?.Name}",
                                    $"Exception: {context.Exception.Message}",
                                    $"Failure?: {context.Result?.Failure?.Message}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogError(logMessage);

                                return Task.CompletedTask;
                            },
                            OnAccessDenied = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var endpoint = context.HttpContext.GetEndpoint();
                                var authorizeData = endpoint?.Metadata.GetOrderedMetadata<AuthorizeAttribute>();

                                var policies = authorizeData?.SelectMany(x => x.Policy?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? []);

                                var roles = authorizeData?.SelectMany(x =>
                                    x.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? []);


                                var logMessage = FormatLogMessage(eventCounter, "Forbidden Access",
                                    $"Scheme: {context.Scheme?.Name}",
                                    $"Request Path: {context.Request?.Path}",
                                    $"Request Method: {context.Request?.Method}",
                                    $"Endpoint policies: {string.Join(",", policies ?? new List<string>())}",
                                    $"Endpoint roles: {string.Join(",", roles ?? new List<string>())}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Token Validated",
                                    $"Scheme: {context.Scheme?.Name}",
                                    $"User: {context.Principal?.Identity?.Name}",
                                    $"Claims: {string.Join(",", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>())}",
                                    $"Token: {context.SecurityToken}",
                                    $"Authentication Type: {context.Principal?.Identity?.AuthenticationType}",
                                    $"Is Authenticated: {context.Principal?.Identity?.IsAuthenticated}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnMessageReceived = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Message Received",
                                    $"Scheme: {context.Scheme?.Name}",
                                    $"Request Path: {context.Request?.Path}",
                                    $"Request Method: {context.Request?.Method}",
                                    $"Token: {context.Token}",
                                    $"AuthenticationProperties: {JsonConvert.SerializeObject(context.Properties)}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnRedirectToIdentityProvider = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Redirect to Identity Provider",
                                    $"AuthorizationEndpoint: {context.ProtocolMessage?.AuthorizationEndpoint}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                //Middleware for Browser! Does not pass through the ConfigurationManager
                                //This happens after this event. Why I do not know ==> TODO: ask on GITHUB
                                context.Properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, context.ProtocolMessage?.RedirectUri);
                                context.ProtocolMessage!.State = context.Options.StateDataFormat.Protect(context.Properties);

                                var authorizationRequestUri = context.ProtocolMessage?.BuildRedirectUrl()!;
                                var mockedAuthorizationCode = backChannelMessageHandler.GetAuthorizationLocationHeaderFromFullUri(authorizationRequestUri);

                                // Improved log message
                                logMessage = $"Mocking answer from OpenId Connect Provider! {mockedAuthorizationCode}";
                                ResourceServerOidc.LogInformation(logMessage);

                                context.HandleResponse();
                                context.Response!.Redirect(mockedAuthorizationCode);

                                return Task.CompletedTask;
                            },
                            OnRedirectToIdentityProviderForSignOut = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Redirect to Identity Provider for SignOut",
                                    $"RedirectUri: {context.ProtocolMessage?.RedirectUri}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnRemoteFailure = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Remote Failure",
                                    $"Result: {context.Result?.Succeeded}",
                                    $"Failure: {context.Result?.Failure?.Message}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogError(logMessage);

                                return Task.CompletedTask;
                            },
                            OnSignedOutCallbackRedirect = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Signed Out Callback Redirect",
                                    $"RedirectUri: {context.ProtocolMessage?.RedirectUri}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnRemoteSignOut = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Remote SignOut",
                                    $"RedirectUri: {context.ProtocolMessage?.RedirectUri}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnTokenResponseReceived = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Token Response Received",
                                    $"Token: {context.TokenEndpointResponse.AccessToken}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnTicketReceived = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Ticket Received",
                                    $"ReturnUri: {context.ReturnUri}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnAuthorizationCodeReceived = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Authorization Code Received",
                                    $"Code: {context.ProtocolMessage.Code}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnUserInformationReceived = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "User Information Received",
                                    $"User: {context.User.RootElement.ToString()}",
                                    $"Response Status Code: {context.Response?.StatusCode}",
                                    $"Response Headers: {BuildHeadersString(context.Response?.Headers)}");

                                ResourceServerOidc?.LogInformation(logMessage);

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
    private int _eventCounter = 0;
    private readonly ConcurrentDictionary<string, int> _sequenceNumbers = new();

    private string BuildHeadersString(IHeaderDictionary? headers)
    {
        StringBuilder headersBuilder = new StringBuilder();
        if (headers != null)
            foreach (var header in headers)
            {
                headersBuilder.AppendLine($"{header.Key}: {header.Value}");
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

                line += $"{word} ";
            }

            builder.AppendLine(line);
        }

        return builder.ToString();
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

    public ILogger? ResourceServerOidc { get; set; } 

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