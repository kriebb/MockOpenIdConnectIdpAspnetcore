using System.Collections.Specialized;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
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
                            OnAuthenticationFailed = context =>
                            {
                                
                                logger.LogInformation("Authentication Failed. Result: {0} Failure:{1}", context.Exception.Message, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnForbidden = context => {

                                logger.LogInformation("Token Validated. Result: {0} Failure:{1}",context.Result?.Succeeded, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {

                                logger.LogInformation("Token Validated. User: {0} Claims:{1}", context.Principal?.Identity?.Name, 
                                    string.Join(",", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
                                return Task.CompletedTask;
                            },
                            OnMessageReceived = context => {

                                logger.LogInformation("Message Received.");
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