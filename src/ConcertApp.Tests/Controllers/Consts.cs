
namespace ConcertApp.Tests.Controllers;

public class Consts
{
    public const string ValidSubClaimValue = "01234-kristof-userididentifier-9087";

    public const string ValidScopeClaimValue = "concert:ticket:buy";

    public const string ValidCountryClaimValue = "Belgium";

    public static string ValidIssuer { get; } = $"Issuer:Dotnet:ConcertApp:Tests:Project";
    public static string ValidAudience { get; }= $"Audience:Dotnet:ConcertApp:Project";
}