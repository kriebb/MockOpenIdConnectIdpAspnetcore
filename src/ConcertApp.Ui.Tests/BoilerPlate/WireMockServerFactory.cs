using WireMock.Server;

namespace ConcertApp.Ui.Tests.BoilerPlate;

public sealed class WireMockServerFactory
{
    public WireMockServer CreateDependency(Func<ITestOutputHelper> testOutputHelper, bool enableRecording = false, string domainUrl = "", string proxy = "http://localhost:8888")
    {
        return new DependencyService(testOutputHelper).CreateDependency(domainUrl
            ,
             proxy, enableRecording);
    }
}