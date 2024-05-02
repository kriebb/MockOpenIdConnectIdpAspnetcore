using System.Diagnostics;
using ConcertApp.Ui.Tests.BoilerPlate;
using Microsoft.Extensions.Logging.Debug;
using NUnit.Framework;

namespace ConcertApp.Ui.Tests;

[SetUpFixture]
public class SetUpConfig
{
    public static PlaywrightCompatibleWebApplicationFactory? WebAppFactory { get; set; }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        Trace.Listeners.Add(new DefaultTraceListener());

        WebAppFactory = new PlaywrightCompatibleWebApplicationFactory();
        



    }

    

    [OneTimeTearDown()]
    
    public async Task OneTimeTearDown()
    {
        if (WebAppFactory != null) await WebAppFactory.DisposeAsync();
    }

}