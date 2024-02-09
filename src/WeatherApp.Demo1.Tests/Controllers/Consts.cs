using WeatherApp.Demo2.Tests.Infrastructure.OpenId;
using WeatherApp.Demo2.Tests.Infrastructure.Security;

namespace WeatherApp.Demo2.Tests.Controllers;

public class Consts
{


    public static int BaseAddressPort { get; set; } = 8443;

    public static readonly Uri BaseAddress = new($"https://localhost:{BaseAddressPort}");

    //TODO: 03_Const_Basic_addition

    //TODO: 07_ConstAddition

    //TODO: 13_AddClaimsToToken

}


