using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace ConcertApp.Tests.Controllers.Models;

public abstract record TokenParameters
{
    public X509Certificate2 SigningCertificate { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public List<Claim> Claims { get; set; }

    public void AddOrReplaceClaim(string claimType, string claimValue)
    {
        var claim = Claims?.FirstOrDefault(x => x.Type == claimType);
        if (claim != null)
            Claims?.Remove(claim);

        Claims ??= new List<Claim>();
        Claims.Add(new Claim(claimType, claimValue));
    }
}