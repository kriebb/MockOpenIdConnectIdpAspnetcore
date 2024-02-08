using System.Text;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using WeatherApp.Ui.Tests.Assets;
using WeatherApp.Ui.Tests.BoilerPlate;

namespace WeatherApp.Ui.Tests;

[Parallelizable(ParallelScope.Self)]
[Explicit("This test is not working yet")]
[TestFixture()]
public class GivenHomePage : PageTest
{

        
    public override BrowserNewContextOptions ContextOptions()
    {
        BrowserNewContextOptions? options = base.ContextOptions();


            options = new()
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
         
        Page.Request += (_, request) => SetUpConfig.UiWebApplicationFactory.TestOutputHelper.WriteLine(">> " + request.Method + " " + request.Url + System.Environment.NewLine + Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>()));
        Page.RequestFailed += (_, request) => SetUpConfig.UiWebApplicationFactory.TestOutputHelper.WriteLine(">> RequestFailed: " + request.Method + " " + request.Url + System.Environment.NewLine + Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>()));
        Page.RequestFinished += (_, request) => SetUpConfig.UiWebApplicationFactory.TestOutputHelper.WriteLine(">> RequestFinished: " + request.Method + " " + request.Url + System.Environment.NewLine + Encoding.Default.GetString(request.PostDataBuffer ?? Array.Empty<byte>()));

        Page.Response += (_, response) => SetUpConfig.UiWebApplicationFactory.TestOutputHelper.WriteLine("<< " + response.Status + " " + response.Url + System.Environment.NewLine);
        

        Page.PageError += (_, error) => SetUpConfig.UiWebApplicationFactory.TestOutputHelper.WriteLine("<< " + error);
        Page.Crash += (_, error) => SetUpConfig.UiWebApplicationFactory.TestOutputHelper.WriteLine("<< " + error);

        Page.SetDefaultTimeout(3000);
        Page.SetDefaultNavigationTimeout(3000);


        var mappingsJsonToLoad = EmbeddedResourceReader.ReadAssets("HomeTests.Assets.Mappings").ToMappings();

        SetUpConfig.UiWebApplicationFactory.UsersApiDependency.ResetMappings();
        SetUpConfig.UiWebApplicationFactory.UsersApiDependency.WithMapping(mappingsJsonToLoad.ToArray());
        await Page.GotoAsync($"{SetUpConfig.UiWebApplicationFactory.ServerAddress}weahterappui");

    }


    [Test]
    public async Task WhenWeClickOnSearch_SearchPageShouldAppear()
    {
        Page.SetDefaultTimeout(30000);
        Page.SetDefaultNavigationTimeout(30000);

        await Page.GotoAsync($"{SetUpConfig.UiWebApplicationFactory.ServerAddress}weahterappui");
        await Page.GetByText("NL").ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Een graad zoeken" }).ClickAsync();

        await Expect(Page.GetByText( "Een graad zoeken")).ToBeInViewportAsync();
    }
}