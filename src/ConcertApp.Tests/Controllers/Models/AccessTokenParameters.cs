using System.Security.Claims;

namespace ConcertApp.Tests.Controllers.Models;

public record AccessTokenParameters: TokenParameters
{

    public AccessTokenParameters()
    {
        Audience = Consts.ValidAudience;

        Issuer = Consts.ValidIssuer;
        SigningCertificate = Consts.ValidSigningCertificate.ToX509Certificate2();
        Claims = new List<Claim>
        {
            new(Consts.SubClaimType, Consts.SubClaimValidValue),
            new(Consts.ScopeClaimType, Consts.ScopeClaimValidValue),
            new(Consts.CountryClaimType,
                Consts.CountryClaimValidValue)

        };
    }
   
}