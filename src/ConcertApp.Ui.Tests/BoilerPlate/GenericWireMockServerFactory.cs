using System.Collections.Specialized;
using ConcertApp.Ui.Tests.BoilerPlate.Json;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;

namespace ConcertApp.Ui.Tests.BoilerPlate;

public class DependencyService(Func<ITestOutputHelper> testOutputHelperFactory)
{
    public WireMockServer CreateDependency(string domain, string? fullProxyHostUri = null, bool enableRecording = false)
    {
        if (!domain.Contains("https"))
        {
            domain = $"https://{domain}";
        }

        var wireMockServerSettings = new WireMockServerSettings
        {
            UseSSL = true,
            FileSystemHandler = new GuidLocalFileSystemHandler()
        };

        if (enableRecording)
        {
            ConfigureRecordingSettings(domain, fullProxyHostUri, wireMockServerSettings);
        }

        var mockedExternalDependency = WireMockServer.Start(wireMockServerSettings);

        SetupLogEntriesHandler(testOutputHelperFactory, mockedExternalDependency);

        return mockedExternalDependency;
    }

    private void ConfigureRecordingSettings(string domain, string? fullProxyHostUri, WireMockServerSettings wireMockServerSettings)
    {
        wireMockServerSettings.StartAdminInterface = true;
        wireMockServerSettings.SaveUnmatchedRequests = true;
        wireMockServerSettings.AllowCSharpCodeMatcher = true;
        wireMockServerSettings.DoNotSaveDynamicResponseInLogEntry = true;
        wireMockServerSettings.Logger = new WireMockConsoleLogger();

        wireMockServerSettings.ProxyAndRecordSettings = new ProxyAndRecordSettings
        {
            Url = domain,
            SaveMappingSettings = new ProxySaveMappingSettings(),
            SaveMapping = true,
            SaveMappingToFile = true,
            ExcludedHeaders = ["Host", "Authorization", "traceparent", "Content-Type", "Content-Length", "Accept"]
        };

        if (fullProxyHostUri != null)
        {
            wireMockServerSettings.ProxyAndRecordSettings.WebProxySettings = new WebProxySettings { Address = fullProxyHostUri };
            wireMockServerSettings.WebhookSettings = new WebhookSettings { WebProxySettings = new WebProxySettings { Address = fullProxyHostUri } };
        }
    }

    private void SetupLogEntriesHandler(Func<ITestOutputHelper> testOutputHelper, WireMockServer mockedExternalDependency)
    {
        mockedExternalDependency.LogEntriesChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (!(item is LogEntry logEntry) || logEntry.ResponseMessage?.StatusCode is null || (int)logEntry.ResponseMessage.StatusCode == 404)
                    {
                        continue;
                    }

                    if (logEntry.PartialMatchResult is not null)
                    {
                        var reasons = logEntry.PartialMatchResult.MatchDetails
                            .Select(x => $"{x?.MatcherType.Name}[Score:{x.Score}]").ToArray();

                        testOutputHelper().WriteLine("Reason: PartialMatchResult:" + string.Join(",", reasons));
                    }

                    if (logEntry.RequestMatchResult is not null)
                    {
                        var reasons = logEntry.RequestMatchResult.MatchDetails
                            .Select(x => $"{x?.MatcherType.Name}[Score:{x.Score}]").ToArray();
                        testOutputHelper().WriteLine("Reason: RequestMatchResult:" + string.Join(",", reasons));
                    }

                    testOutputHelper().WriteLine("RequestMessage: AbsoluteUrl: " + logEntry.RequestMessage.AbsoluteUrl);
                    testOutputHelper().WriteLine("RequestMessage: Body: " + JsonConvert.SerializeObject(logEntry.RequestMessage.Body));
                }
            }
        };
    }
}