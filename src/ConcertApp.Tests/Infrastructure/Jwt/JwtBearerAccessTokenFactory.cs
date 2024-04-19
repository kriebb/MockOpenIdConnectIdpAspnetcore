using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ConcertApp.Tests.Controllers.Models;
using Microsoft.IdentityModel.Tokens;

namespace ConcertApp.Tests.Infrastructure.Jwt;

public static class JwtBearerAccessTokenFactory
{
    public static string Create(TokenParameters tokenParameters)
    {
        var signingCredentials = new SigningCredentials(new X509SecurityKey(tokenParameters.SigningCertificate),
            SecurityAlgorithms.RsaSha256);

        var notBefore = DateTime.UtcNow;
        var expires = DateTime.UtcNow.AddHours(1);


        var identity = new ClaimsIdentity(tokenParameters.Claims);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = tokenParameters.Audience,
            Issuer = tokenParameters.Issuer,
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


    public static string CreateRefreshToken()
    {
        // This is a simple method to create a secure random string for a refresh token.
        // In a production system, you may need to include additional logic to manage issuance,
        // storage, revocation, and security of refresh tokens.
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(randomBytes);

        return refreshToken;

    }

}

