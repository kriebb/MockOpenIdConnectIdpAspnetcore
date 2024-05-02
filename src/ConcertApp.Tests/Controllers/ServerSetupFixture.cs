using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics.Tracing;
using System.Text;
using ConcertApp.Ui.Tests.BoilerPlate.Json;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using OIdcMockingInfrastructure.Jwt;
using OIdcMockingInfrastructure.Models;
using OIdcMockingInfrastructure.OpenId;
using Xunit.Abstractions;

namespace ConcertApp.Tests.Controllers;


public sealed class ServerSetupFixture : WebApplicationFactory<Program>
{
    private Func<ITestOutputHelper?> _testOutputHelper = () => null;
    private int _eventCounter = 0;
    private readonly ConcurrentDictionary<string, int> _sequenceNumbers = new();

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
            var words = detail.Split(' ');

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


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;


        
        builder.ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.AddJsonFile("appsettings.json");
                configuration.AddJsonFile("appsettings.Development.json");
                configuration.AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Jwt:Audience", Consts.ValidAudience },
                    { "Jwt:Issuer", Consts.ValidIssuer }
                }!);
            })
            .ConfigureKestrel((context, options) =>
                {

                    options.ListenLocalhost(OIdcMockingInfrastructure.Consts.BaseAddressPort);
                }
            )
            .ConfigureLogging((context, loggingBuilder) =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddXUnit(new TestOutputHelperFuncAccessor(_testOutputHelper));
            })
            .ConfigureTestServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        var logger = Services.GetRequiredService<ILogger<ServerSetupFixture>>();

                        options.ConfigurationManager = ConfigForMockedOpenIdConnectServer.Create(Consts.ValidIssuer);
                        options.IncludeErrorDetails = true;
                        options.Events = new JwtBearerEvents()
                        {
                            OnChallenge = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Challenge Issued",
                                    $"Scheme: {context.Scheme?.Name}",
                                    $"Request Path: {context.Request?.Path}",
                                    $"Request Method: {context.Request?.Method}",
                                    $"Error?: {context.Error}",
                                    $"Error Description?: {context.ErrorDescription}",
                                    $"Exception Message?: {context.AuthenticateFailure?.Message}"

                                    );

                                logger.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnAuthenticationFailed = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Authentication Failed",
                                    $"Scheme: {context.Scheme?.Name}",
                                    $"Exception: {context.Exception.Message}",
                                    $"Failure?: {context.Result?.Failure?.Message}");

                                logger.LogError(logMessage);

                                return Task.CompletedTask;
                            },
                            OnForbidden = context =>
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
                                    $"Endpoint policies: {string.Join(",", policies ?? new List<string>())}",         $"Endpoint roles: {string.Join(",", roles ?? new List<string>())}");


                                logger.LogInformation(logMessage);

                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                var eventCounter = IncrementEventCounter(context.HttpContext.TraceIdentifier);
                                var logMessage = FormatLogMessage(eventCounter, "Token Validated",
                                    $"Scheme: {context.Scheme?.Name}",
                                    $"User: {context.Principal?.Identity?.Name}",
                                    $"Claims: {string.Join(",", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>())}",
                                    $"Token: {context.SecurityToken}");

                                logger.LogInformation(logMessage);

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
                                    $"AuthenticationProperties: {JsonConvert.SerializeObject(context.Properties)}");

                                logger.LogInformation(logMessage);

                                return Task.CompletedTask;
                            }
                        };

                    });
            });
    }


    /// <summary>
    /// Clears the current <see cref="ILogger"/>.
    /// </summary>
    public void ClearOutputHelper()
    {
        _testOutputHelper = () => null;
    }

    /// <summary>
    /// Sets the <see cref="ILogger"/> to use.
    /// </summary>
    /// <param name="value">The <see cref="ILogger"/> to use.</param>
    public void SetOutputHelper(ITestOutputHelper value)
    {
        _testOutputHelper = () => value;
    }






    protected override void ConfigureClient(HttpClient client)
    {
        client.BaseAddress = OIdcMockingInfrastructure.Consts.BaseAddress;

    }


}