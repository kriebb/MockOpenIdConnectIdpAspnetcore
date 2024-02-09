using System;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using WeatherApp.Demo.Tests.Infrastructure.OpenId;
using WireMock.Admin.Mappings;
using WireMock.Admin.Requests;
using WireMock.Logging;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Xunit.Abstractions;

namespace WeatherApp.Demo.Tests.Controllers;


public sealed class WeatherForecastServerSetupFixture : WebApplicationFactory<Program>
{
    public WeatherForecastServerSetupFixture()
    {


    }
    private Func<ITestOutputHelper?> _testOutputHelper = () => null;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;


        builder.ConfigureAppConfiguration((context, configuration) =>
            {
                //TODO 02_01_ServerSetupFixture_AddUrl

            })
            .ConfigureKestrel((context, options) =>
                {

                    options.ListenLocalhost(Consts.BaseAddressPort);
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
                        //TODO 03_ServerSetupFixture_RemoveConfig
                        options.ConfigurationManager = ConfigForMockedOpenIdConnectServer.Create();
                        //
                        options.IncludeErrorDetails = true;
                        options.Events = new JwtBearerEvents()
                        {
                            OnAuthenticationFailed = context =>
                            {
                                var testOutputHelper = Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper;

                                testOutputHelper?.WriteLine("Authentication Failed. Result: {0} Failure:{1}", context.Exception.Message, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnForbidden = context =>
                            {
                                var testOutputHelper = Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper;

                                testOutputHelper?.WriteLine("Token Validated. Result: {0} Failure:{1}", context.Result?.Succeeded, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                var testOutputHelper = Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper;

                                testOutputHelper?.WriteLine("Token Validated. User: {0} Claims:{1}", context.Principal?.Identity?.Name,
                                    string.Join(",", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
                                return Task.CompletedTask;
                            },
                            OnMessageReceived = context =>
                            {
                                var testOutputHelper = Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper;

                                testOutputHelper?.WriteLine("Message Received.");
                                return Task.CompletedTask;
                            }
                        };
                    });
            });
    }


    protected override void ConfigureClient(HttpClient client)
    {
        client.BaseAddress = Consts.BaseAddress;

    }

    /// <summary>
    /// Clears the current <see cref="ITestOutputHelper"/>.
    /// </summary>
    public void ClearOutputHelper()
    {
        _testOutputHelper = () => null;
    }

    //TODO 01_ServerSetupFixture_CreateHostWithWireMockOIDC

    /// <summary>
    /// Sets the <see cref="ITestOutputHelper"/> to use.
    /// </summary>
    /// <param name="value">The <see cref="ITestOutputHelper"/> to use.</param>
    public void SetOutputHelper(ITestOutputHelper value)
    {
        _testOutputHelper = () => value;
    }

    private sealed class TestOutputHelperFuncAccessor
        (Func<ITestOutputHelper?> testOutputHelper) 
        : ITestOutputHelperAccessor
    {
        public ITestOutputHelper? OutputHelper
        {
            get => testOutputHelper.Invoke();
            set { testOutputHelper = () => value; }
        }
    }

}
