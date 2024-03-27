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
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using WeatherApp.Tests.Controllers.Models;
using WeatherApp.Tests.Infrastructure.Jwt;
using WeatherApp.Tests.Infrastructure.OpenId;
using Xunit.Abstractions;

namespace WeatherApp.Tests.Controllers;


public sealed class WeatherForecastServerSetupFixture : WebApplicationFactory<Program>
{
    private Func<ITestOutputHelper?> _testoutputhelper = () => null;


    public (string AccessToken, string IDToken, string RefreshToken) TokenFactoryFunc(
        (NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery) arg)
    {
        var accessToken = JwtBearerAccessTokenFactory.Create(new AccessTokenParameters());
        var idToken = JwtBearerAccessTokenFactory.Create(new IdTokenParameters(
            sub: arg.AuthorizationCodeRequestQuery[""]!,
            nonce: arg.AuthorizationCodeRequestQuery["nonce"]!,
            scopes: arg.AuthorizationCodeRequestQuery["scope"]!));
        var refreshToken = JwtBearerAccessTokenFactory.CreateRefreshToken();
        return (accessToken, idToken, refreshToken);
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
                });
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
                loggingBuilder.AddXUnit(new TestOutputHelperFuncAccessor(_testoutputhelper));
            })
            .ConfigureTestServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.ConfigurationManager = ConfigForMockedOpenIdConnectServer.Create(TokenFactoryFunc);
                        options.IncludeErrorDetails = true;
                        options.Events = new JwtBearerEvents()
                        {
                            OnAuthenticationFailed = context =>
                            {
                                var testOutputHelper = Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper;
                                
                                testOutputHelper?.WriteLine("Authentication Failed. Result: {0} Failure:{1}", context.Exception.Message, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnForbidden = context => {
                                var testOutputHelper = Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper;

                                testOutputHelper?.WriteLine("Token Validated. Result: {0} Failure:{1}",context.Result?.Succeeded, context.Result?.Failure?.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                var testOutputHelper = Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper;

                                testOutputHelper?.WriteLine("Token Validated. User: {0} Claims:{1}", context.Principal?.Identity?.Name, 
                                    string.Join(",", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
                                return Task.CompletedTask;
                            },
                            OnMessageReceived = context => {
                                var testOutputHelper = Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper;

                                testOutputHelper?.WriteLine("Message Received.");
                                return Task.CompletedTask;
                            }
                        };
                    });
            });
    }


    /// <summary>
    /// Clears the current <see cref="ITestOutputHelper"/>.
    /// </summary>
    public void ClearOutputHelper()
    {
        _testoutputhelper = () => null;
    }

    /// <summary>
    /// Sets the <see cref="ITestOutputHelper"/> to use.
    /// </summary>
    /// <param name="value">The <see cref="ITestOutputHelper"/> to use.</param>
    public void SetOutputHelper(ITestOutputHelper value)
    {
        _testoutputhelper = () => value;
    }






    protected override void ConfigureClient(HttpClient client)
    {
        client.BaseAddress = Consts.BaseAddress;

    }


}

public class TestOutputHelperFuncAccessor : ITestOutputHelperAccessor
{
    private Func<ITestOutputHelper?> _testoutputhelper;

    public TestOutputHelperFuncAccessor(Func<ITestOutputHelper?> testoutputhelper)
    {
        _testoutputhelper = testoutputhelper;
    }

    public ITestOutputHelper? OutputHelper
    {
        get { return _testoutputhelper.Invoke();}
        set { _testoutputhelper = () => value; }
    } 
}
