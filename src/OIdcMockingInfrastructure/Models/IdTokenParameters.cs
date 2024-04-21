using System.Security.Claims;

namespace OIdcMockingInfrastructure.Models;

public record IdTokenParameters: TokenParameters
{

    public IdTokenParameters(string sub, string nonce, string scopes, string audience, string issuer, string countryClaimValidValue)
    {
        Audience = audience;

        Issuer = issuer;
        SigningCertificate = Consts.ValidSigningCertificate.ToX509Certificate2();
        Claims = new List<Claim>
        {
            new(Consts.SubClaimType, sub),
            new(Consts.ScopeClaimType, scopes),
            new(Consts.CountryClaimType,
                countryClaimValidValue),
            new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new("nonce", nonce)
        };
    }
   
}