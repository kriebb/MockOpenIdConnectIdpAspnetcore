namespace WeatherApp.Ui.Tests.BoilerPlate;

public readonly struct OidcServer
{
    public string Url { get; }

    public OidcServer(string url)
    {
        Url = url;
    }
}