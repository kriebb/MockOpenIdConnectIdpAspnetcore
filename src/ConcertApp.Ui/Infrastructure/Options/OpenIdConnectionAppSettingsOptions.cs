using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace ConcertApp.Ui.Infrastructure.Options;

public record OpenIdConnectionAppSettingsOptions
{
    [Required]
    public string ClientId { get; set; } = default!;

    [Required]
    public string ClientSecret { get; set; } = default!;

    [Required] public string SignInScheme { get; set; } = IdentityConstants.ExternalScheme;

    [Required]
    public string Authority { get; set; } = default!;

    [Required]
    public string ValidIssuer { get; set; } = default!;

    [Required]
    public string ValidAudience { get; set; } = default!;

    [Required]
    public string Prompt { get; set; } = default!;

    [Required]
    public bool? UsePkce { get; set; } = default!;

    [Required]
    public string ResponseMode { get; set; } = default!;

    [Required]
    public string ResponseType { get; set; } = default!;

    [Required]
    public string Scope { get; set; } = default!;

    [Required]
    public bool? MapInboundClaims { get; set; } = default!;

    [Required]
    public bool? GetClaimsFromUserInfoEndpoint { get; set; } = default!;

    [Required]
    public string ClaimName { get; set; } = default!;

    public void Validate()
    {
        var context = new ValidationContext(this, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();


        if (!Validator.TryValidateObject(this, context, results, true))
        {
            foreach (var validationResult in results)
            {

                Console.Error.WriteLine(validationResult.ErrorMessage);
            }

            throw new InvalidOperationException(FormatValidationErrors(results));
        }
    }
    
    private static string FormatValidationErrors(IEnumerable<ValidationResult> validationResults)
    {
        var stringBuilder = new StringBuilder("Configuration validation failed:");

        foreach (var validationResult in validationResults)
        {
            var memberNames = string.Join(";", validationResult.MemberNames);
            stringBuilder.Append($"[{validationResult.ErrorMessage}: {memberNames}]");
        }

        return stringBuilder.ToString();
    }
}