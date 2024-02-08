using NUnit.Framework;
using WeatherApp.Ui.Tests.BoilerPlate;

namespace WeatherApp.Ui.Tests;

[SetUpFixture]
public class SetUpConfig
{
    public static PlaywrightCompatibleWebApplicationFactory UiWebApplicationFactory { get; set; }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        UiWebApplicationFactory = new PlaywrightCompatibleWebApplicationFactory();
        



    }

    

    [OneTimeTearDown()]
    
    public async Task OneTimeTearDown()
    {
       await UiWebApplicationFactory.DisposeAsync();
    }

}