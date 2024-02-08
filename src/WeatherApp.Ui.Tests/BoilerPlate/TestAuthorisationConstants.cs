namespace WeatherApp.Ui.Tests.BoilerPlate;

public sealed class TestAuthorisationConstants
{

    public static string Audience { get; set; } = "urn:weatherapp:ui:v1";
    public static string Issuer { get; set; } = "https://login.microsoft.com"; //Authority
    public static string Domain { get; set; } = "inmemory.eu.oidc.com";
    public static string Subject { get; set; } = "me";

    public static string SigningAlgorithm { get; set; } = "RS256";
}