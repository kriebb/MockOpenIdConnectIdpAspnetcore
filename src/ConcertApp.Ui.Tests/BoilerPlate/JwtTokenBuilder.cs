using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace WeatherApp.Ui.Tests.BoilerPlate;

public sealed class JwtTokenBuilder
{
    private readonly List<Claim> _claims = new();
    private readonly JwtSecurityTokenHandler _securityTokenHandler = new();
    private string _audience = "https://subject.com";
    private string _issuer = "https://issuer.com";
    private TimeSpan _life = TimeSpan.FromHours(1);
    private DateTime _notBefore = DateTime.UtcNow;
    private X509Certificate2 _signingCertificate;

    public JwtTokenBuilder IssuedBy(string issuer)
    {
        if (string.IsNullOrEmpty(issuer))
            throw new ArgumentException("Issued by cannot be null or empty", nameof(issuer));

        _issuer = issuer;

        return this;
    }

    public JwtTokenBuilder ForAudience(string audience)
    {
        if (string.IsNullOrEmpty(audience))
            throw new ArgumentException("Audience cannot be null or empty", nameof(audience));

        _audience = audience;

        return this;
    }

    public JwtTokenBuilder ForSubject(string subject)
    {
        if (string.IsNullOrEmpty(subject))
            throw new ArgumentException("Subject cannot be null or empty", nameof(subject));

        if (_claims.FirstOrDefault(claim => claim.Type == "sub") == null) _claims.Add(new Claim("sub", subject));

        return this;
    }

    public JwtTokenBuilder WithSigningCertificate(X509Certificate2 certificate)
    {
        _signingCertificate = certificate ??
                              throw new ArgumentException("Certificate cannot be null or empty", nameof(certificate));

        return this;
    }

    public JwtTokenBuilder WithClaim(string claimType, string value)
    {
        if (string.IsNullOrEmpty(claimType))
            throw new ArgumentException("Claim type cannot be null or empty", nameof(claimType));

        if (value == null) value = string.Empty;

        _claims.Add(new Claim(claimType, value));

        return this;
    }

    public JwtTokenBuilder WithClaim(Claim claim)
    {
        _claims.Add(claim);

        return this;
    }

    public JwtTokenBuilder WithLifetime(TimeSpan life)
    {
        _life = life;

        return this;
    }

    public JwtTokenBuilder NotBefore(DateTime notBefore)
    {
        _notBefore = notBefore;

        return this;
    }

    public string BuildToken()
    {
        if (_signingCertificate == null)
            throw new InvalidOperationException(
                "You must specify an X509 certificate to use for signing the JWT Token");

        var signingCredentials =
            new SigningCredentials(new X509SecurityKey(_signingCertificate), SecurityAlgorithms.RsaSha256);

        var notBefore = _notBefore;
        var expires = notBefore.Add(_life);

        var identity = new ClaimsIdentity(_claims);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = _audience,
            Issuer = _issuer,
            NotBefore = notBefore,
            Expires = expires,
            SigningCredentials = signingCredentials,
            Subject = identity
        };

        var token = _securityTokenHandler.CreateToken(securityTokenDescriptor);

        var encodedAccessToken = _securityTokenHandler.WriteToken(token);

        return encodedAccessToken;
    }
}