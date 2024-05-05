
namespace WeatherApp.Demo1.Tests.Controllers;

public class Consts
{


    public static int BaseAddressPort { get; set; } = 8443;

    public static readonly Uri BaseAddress = new($"https://localhost:{BaseAddressPort}");

}