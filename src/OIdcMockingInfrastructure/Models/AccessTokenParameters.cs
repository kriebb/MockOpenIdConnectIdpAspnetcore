using System.Security.Claims;

namespace OIdcMockingInfrastructure.Models;

public record AccessTokenParameters : TokenParameters
{

    public AccessTokenParameters(string validAudience, string validIssuer, string validSubClaimValue,
        string validScopeClaimValue, string validCountryClaimValue)
    {
        Audience = validAudience;

        Issuer = validIssuer;
        SigningCertificate = Consts.ValidSigningCertificate.ToX509Certificate2();
        Claims = new List<Claim>
        {
            new(Consts.SubClaimType, validSubClaimValue),
            new(Consts.ScopeClaimType, validScopeClaimValue),
            new(Consts.CountryClaimType,
                validCountryClaimValue)

        };

    }

}