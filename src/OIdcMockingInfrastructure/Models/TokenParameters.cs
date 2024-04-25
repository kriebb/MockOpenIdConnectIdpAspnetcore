using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace OIdcMockingInfrastructure.Models;

public abstract record TokenParameters
{
    public X509Certificate2 SigningCertificate { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public List<Claim> Claims { get; set; }
    
    public DateTime NotBefore { get; set; }= DateTime.UtcNow;
    public DateTime Expires { get; set; }= DateTime.UtcNow.AddHours(1);

    public TokenParameters AddOrReplaceClaim(string claimType, string claimValue)
    {
        var claim = Claims?.FirstOrDefault(x => x.Type == claimType);
        if (claim != null)
            Claims?.Remove(claim);

        Claims ??= new List<Claim>();
        Claims.Add(new Claim(claimType, claimValue));

        return this;
    }
}