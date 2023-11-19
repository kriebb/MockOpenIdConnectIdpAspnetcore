using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace WeatherApp.Tests.Controllers.Models;

public record AccessTokenParameters
{
    public X509Certificate2 SigningCertificate { get; set; } = Consts.ValidSigningCertificate.ToX509Certificate2();
    public string Audience { get; set; } = Consts.ValidAudience;
    public string Issuer { get; set; } = Consts.ValidIssuer;
    public List<Claim> Claims { get; set ; } = new()
    {
        new(Consts.SubClaimType, Consts.SubClaimValidValue),
        new(Consts.ScopeClaimType, Consts.ScopeClaimValidValue),
        new(Consts.CountryClaimType,
            Consts.CountryClaimValidValue)

    };

    public void AddOrReplaceClaim(string claimType, string claimValue)
    {
        var claim = Claims?.FirstOrDefault(x => x.Type == claimType);
        if (claim != null)
            Claims?.Remove(claim);
     
        Claims ??= new List<Claim>();
        Claims.Add(new Claim(claimType,claimValue));
    }
}