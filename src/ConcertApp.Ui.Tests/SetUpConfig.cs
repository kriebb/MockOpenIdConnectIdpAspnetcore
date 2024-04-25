using ConcertApp.Ui.Tests.BoilerPlate;
using NUnit.Framework;

namespace ConcertApp.Ui.Tests;

[SetUpFixture]
public class SetUpConfig
{
    public static PlaywrightCompatibleWebApplicationFactory WebAppFactory { get; set; }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        WebAppFactory = new PlaywrightCompatibleWebApplicationFactory();
        



    }

    

    [OneTimeTearDown()]
    
    public async Task OneTimeTearDown()
    {
       await WebAppFactory.DisposeAsync();
    }

}