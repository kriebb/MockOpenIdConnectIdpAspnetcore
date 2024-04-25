using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OIdcMockingInfrastructure.Models;

namespace OIdcMockingInfrastructure.Jwt;

public static class JwtTokenFactory
{
    public static string Create(TokenParameters tokenParameters)
    {
        var signingCredentials = new SigningCredentials(new X509SecurityKey(tokenParameters.SigningCertificate),
            SecurityAlgorithms.RsaSha256);




        var identity = new ClaimsIdentity(tokenParameters.Claims);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = tokenParameters.Audience,
            Issuer = tokenParameters.Issuer,
            NotBefore = tokenParameters.NotBefore,
            Expires = tokenParameters.Expires,
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

