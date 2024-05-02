using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using ConcertApp.Ui.Tests.BoilerPlate.Json;
using Microsoft.Extensions.Logging;
using OIdcMockingInfrastructure.Models;

namespace OIdcMockingInfrastructure.Jwt;

public class JwtBearerCustomAccessTokenHandler(
    TokenParameters tokenParameters,
    ILogger<JwtBearerCustomAccessTokenHandler> logger)
    : DelegatingHandler
{
    private readonly TokenParameters _tokenParameters = tokenParameters ?? throw new ArgumentNullException(nameof(tokenParameters));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SetAuthorizationHeader(request);
        return base.Send(request, cancellationToken);
    }

    private void SetAuthorizationHeader(HttpRequestMessage request)
    {
        var encodedAccessToken = JwtTokenFactory.Create(_tokenParameters);
        /* For demo purposes, we decode the access token and log its parts */
        var token = new JwtSecurityTokenHandler().ReadJwtToken(encodedAccessToken);


        var parts = encodedAccessToken.Split('.');
        var header = parts[0];
        var payload = parts[1];
        var signatureAlgorithm = parts[2];

        var logMessage = FormatLogMessage(0, "Showing raw information",
            $"*Header* {header}",
            $"*Payload* {payload}",
            $"*Signature* {signatureAlgorithm}");

        _logger.LogInformation(logMessage);

        header = FormatDictionary(JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(token.Header)));
        payload = FormatDictionary(JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(token.Payload)));
        signatureAlgorithm = "Kid: " + token.Header.Kid;

        logMessage = FormatLogMessage(1, "Showing decoded information",
            $"*Header*{System.Environment.NewLine}{header}",
            $"*Payload*{System.Environment.NewLine}{payload}",
            $"*Signature*{System.Environment.NewLine}{signatureAlgorithm}");


        _logger.LogInformation(logMessage);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", encodedAccessToken);
    }


    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SetAuthorizationHeader(request);

        return base.SendAsync(request, cancellationToken);
    }

    //Demo purposes
    private string FormatDictionary(Dictionary<string, object>? dictionary)
    {
        var builder = new StringBuilder();
        if (dictionary != null)
            foreach (var pair in dictionary)
            {
                builder.AppendLine($"{pair.Key}: {pair.Value}");
            }

        return builder.ToString();
    }
    private string FormatLogMessage(int eventCounter, string eventName, params string[] details)
    {
        const string indent = "    ";
        const string indentHeader = "  ";

        var builder = new StringBuilder();
        builder.AppendLine($"{eventCounter}. {eventName}");

        foreach (var detail in details)
        {
            var lines = detail.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if(line.StartsWith("*"))
                    builder.AppendLine($"{indentHeader}{line}");
                else
                    builder.AppendLine($"{indent}{line}");
            }
        }

        return builder.ToString();
    }

}