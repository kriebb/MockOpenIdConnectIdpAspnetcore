namespace ConcertApp.Ui.Tests;

public sealed class Constants
{ 
    public static string ValidCountryClaimValue { get; set; } = "Belgium";

    public static string ValidIssuer { get; } = $"Issuer:Dotnet:ConcertApp:Tests:Project";
    public static string ValidAudience { get; }= $"Audience:Dotnet:ConcertApp:Project";

    public static string ODataContext { get; set; } = "https://graph.microsoft.com/v1.0/$metadata#users/$entity";
    public static string[] BusinessPhones { get; } = new string[0];
    public static string DisplayName { get; set; }= "Kristof Riebbels";
    public static string GivenName { get; set; }= "Kristof";
    public static string JobTitle { get; set; }= null;
    public static string Mail { get; set; }= "Kristof.Riebbels@xebia.com";
    public static string MobilePhone { get; set; }= null;
    public static string OfficeLocation { get; set; }= null;
    public static string PreferredLanguage { get; set; }= null;
    public static string Surname { get; set; }= "Riebbels";
    public static string UserPrincipalName { get; set; }= "Kristof.Riebbels@xebia.com";
    public static string UserId { get; set; }= "06018149-8367-4a03-942c-11c2db3daa6f";
    public static string ValidAuthority { get; set; } = "https://i.do.not.exist.com/";
}