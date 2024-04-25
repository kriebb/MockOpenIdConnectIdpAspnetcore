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
    private Func<ITestOutputHelper?> _testoutputhelper = () => null;

    public Token TokenFactoryFunc(
        (NameValueCollection AuthorizationCodeRequestQuery, NameValueCollection TokenRequestQuery) arg)
    {
        
        var accessToken = JwtTokenFactory.Create(
            new AccessTokenParameters(
                Consts.ValidAudience, 
                Consts.ValidIssuer, 
                Consts.ValidSubClaimValue, 
                arg.AuthorizationCodeRequestQuery["scope"]!,
                Consts.ValidCountryClaimValue));

        var idToken = "not supported for this api. OAuth2 only";
        var refreshToken = "not supported for this api. OAuth2 only";

        return new Token
        (
            AccessToken : accessToken,
            IdToken : idToken,
            RefreshToken : refreshToken
        );
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
                loggingBuilder.AddXUnit(new TestOutputHelperFuncAccessor(_testoutputhelper));
            })
            .ConfigureTestServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.ConfigurationManager = ConfigForMockedOpenIdConnectServer.Create(Consts.ValidIssuer,TokenFactoryFunc, () => throw new NotSupportedException("There is no userinfoEndpoint for this api"));
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
        client.BaseAddress = OIdcMockingInfrastructure.Consts.BaseAddress;

    }


}