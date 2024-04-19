using System.Security.Claims;

namespace ConcertApp.Tests.Controllers.Models;

public record IdTokenParameters: TokenParameters
{

    public IdTokenParameters(string sub, string nonce, string scopes)
    {
        Audience = Consts.ValidAudience;

        Issuer = Consts.ValidIssuer;
        SigningCertificate = Consts.ValidSigningCertificate.ToX509Certificate2();
        Claims = new List<Claim>
        {
            new(Consts.SubClaimType, sub),
            new(Consts.ScopeClaimType, scopes),
            new(Consts.CountryClaimType,
                Consts.CountryClaimValidValue),
            new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("nonce", nonce)
        };
    }
   
}