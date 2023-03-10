using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace WeatherApp.Tests.Controllers.Models;

public record AccessTokenParameters
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
    public X509Certificate2 SigningCertificate { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public List<Claim> Claims { get; set ; } 
    
    public void AddOrReplaceClaim(string claimType, string claimValue)
    {
        var claim = Claims?.FirstOrDefault(x => x.Type == claimType);
        if (claim != null)
            Claims?.Remove(claim);
     
        Claims ??= new List<Claim>();
        Claims.Add(new Claim(claimType,claimValue));
    }
}