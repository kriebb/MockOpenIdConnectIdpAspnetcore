using WireMock.Server;

namespace WeatherApp.Ui.Tests.BoilerPlate;

public sealed class WireMockServerFactory
{
    public WireMockServer CreateDependency(Func<ITestOutputHelper> testOutputHelper, bool enableRecording = false, string domainUrl = "", string proxy = "http://localhost:8888")
    {
        return new GenericWireMockServerFactory().CreateDependency(   domainUrl
            ,
            testOutputHelper, proxy, enableRecording);
    }
}