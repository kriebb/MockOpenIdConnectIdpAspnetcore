using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using WeatherApp.Demo.Tests.Controllers;

namespace WeatherApp.Demo.Tests.Infrastructure.Jwt;
//Demo 2 INSERT BELOW
public class JwtBearerAccessTokenFactory
{
    public static string Create(AccessTokenParameters accessTokenParameters)
    {
        var signingCredentials = new SigningCredentials(new X509SecurityKey(accessTokenParameters.SigningCertificate), SecurityAlgorithms.RsaSha256);

        var notBefore = DateTime.UtcNow;
        var expires = DateTime.UtcNow.AddHours(1);


        var identity = new ClaimsIdentity(accessTokenParameters.Claims);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = accessTokenParameters.Audience,
            Issuer = accessTokenParameters.Issuer,
            NotBefore = notBefore,
            Expires = expires,
            SigningCredentials = signingCredentials,
            Subject = identity,
        };

        var securityTokenHandler = new JwtSecurityTokenHandler();
        var securityToken = securityTokenHandler.CreateToken(securityTokenDescriptor);

        var encodedAccessToken = securityTokenHandler.WriteToken(securityToken);

        return encodedAccessToken;
    }
}