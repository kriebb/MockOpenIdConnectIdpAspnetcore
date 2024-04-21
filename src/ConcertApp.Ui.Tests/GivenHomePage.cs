using System.Text;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using WeatherApp.Ui.Tests.Assets;
using WeatherApp.Ui.Tests.BoilerPlate;

namespace WeatherApp.Ui.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture()]
public class GivenHomePage : PageTest
{

        
    public override BrowserNewContextOptions ContextOptions()
    {
        BrowserNewContextOptions? options = new()
            {
                IgnoreHTTPSErrors = true,
                RecordHarMode = HarMode.Full,
                RecordHarContent = HarContentPolicy.Embed,
                RecordHarPath = @"c:\tools\har.har",
            };
        

        return options;
    }


    [SetUp]
    public async Task Setup()
    {
         
        Page.Request += (_, request) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine(">> " + request.Method + " " + request.Url + System.Environment.NewLine + Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>()));
        Page.RequestFailed += (_, request) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine(">> RequestFailed: " + request.Method + " " + request.Url + System.Environment.NewLine + Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>()));
        Page.RequestFinished += (_, request) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine(">> RequestFinished: " + request.Method + " " + request.Url + System.Environment.NewLine + Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>()));

        Page.Response += (_, response) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine("<< " + response.Status + " " + response.Url + System.Environment.NewLine);
        

        Page.PageError += (_, error) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine("<< " + error);
        Page.Crash += (_, error) => SetUpConfig.WebAppFactory.TestOutputHelper.WriteLine("<< " + error);

        Page.SetDefaultTimeout(3000);
        Page.SetDefaultNavigationTimeout(3000);
        
        SetUpConfig.WebAppFactory.ConcertsApiDependency.ResetMappings();
        await Page.GotoAsync($"{SetUpConfig.WebAppFactory.ServerAddress}");

    }


    [Test]
    public async Task WhenWeClickOnSearch_SearchPageShouldAppear()
    {
        Page.SetDefaultTimeout(30000);
        Page.SetDefaultNavigationTimeout(30000);

        await Page.GotoAsync($"{SetUpConfig.WebAppFactory.ServerAddress}weahterappui");
        await Page.GetByText("NL").ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Een graad zoeken" }).ClickAsync();

        await Expect(Page.GetByText( "Een graad zoeken")).ToBeInViewportAsync();
    }
}