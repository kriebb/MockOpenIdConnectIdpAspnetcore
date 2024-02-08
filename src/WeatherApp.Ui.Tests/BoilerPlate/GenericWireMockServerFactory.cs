using System.Collections.Specialized;
using System.Diagnostics;
using NUnit.Framework;
using WeatherApp.Ui.Tests.BoilerPlate.Json;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;

namespace WeatherApp.Ui.Tests.BoilerPlate;

public sealed class GenericWireMockServerFactory
{
    public WireMockServer CreateDependency(string domain, Func<ITestOutputHelper> testOutputHelper, string fullProxyHostUri = null, bool enableRecording = false)
    {
        if (!domain.Contains("https")) domain = $"https://{domain}";
        var wireMockServerSettings = new WireMockServerSettings
        {
            UseSSL = true,
            FileSystemHandler = new GuidLocalFileSystemHandler()
        };
        var keyPem = File.ReadAllText(@".\Assets\AspNetDevServer\localhost.key");
        wireMockServerSettings.CertificateSettings = new WireMockCertificateSettings
        {
            X509CertificateFilePath = @".\Assets\AspNetDevServer\localhost.pem",
            X509CertificatePassword = keyPem
        };

        //Do not run on CI
        if (Environment.GetEnvironmentVariable("SYSTEM_DEFINITIONID") == null)
        {
            Trace.WriteLine("NO build server detected");
        }
        else
        {
            Trace.WriteLine("Build server detected. Never contact the real apis");

            //when it runs on the buildserver, always NOT RECORD
            enableRecording = false;
        }

        if (enableRecording)
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
                ExcludedHeaders = new[]
                {
                    "Host", "Authorization", "X-Axa-Context", "Auth0-Client", "traceparent", "Content-Type",
                    "Content-Length", "Accept"
                }
            };
            if (fullProxyHostUri != null)
            {
                wireMockServerSettings.ProxyAndRecordSettings.WebProxySettings = new WebProxySettings
                {
                    Address = fullProxyHostUri
                };

                wireMockServerSettings.WebhookSettings = new WebhookSettings
                {
                    WebProxySettings = new WebProxySettings
                    {
                        Address = fullProxyHostUri
                    }
                };
            }

            ;
        }

        var auth0ServerDependency = WireMockServer.Start(wireMockServerSettings);

        auth0ServerDependency.LogEntriesChanged += (sender, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var logEntry = (LogEntry)item;
                    if (logEntry.ResponseMessage.StatusCode ==null || (int)logEntry.ResponseMessage.StatusCode == 404)
                    {
                        if (logEntry.PartialMatchResult != null)
                        {
                            string[] reasons = logEntry.PartialMatchResult.MatchDetails
                                .Select(x => $"{x?.MatcherType.Name}[Score:{x.Score}]").ToArray();
                            
                            testOutputHelper().WriteLine("Reason: PartialMatchResult:" + string.Join(",",reasons));
                        }
                        
                        if (logEntry.RequestMatchResult != null)
                        {
                            string[] reasons = logEntry.RequestMatchResult.MatchDetails
                                .Select(x => $"{x?.MatcherType.Name}[Score:{x.Score}]").ToArray();
                            testOutputHelper().WriteLine("Reason: RequestMatchResult:" + string.Join(",",reasons));

                        }
                        
                        testOutputHelper().WriteLine("RequestMessage: AbsoluteUrl: "+ logEntry.RequestMessage.AbsoluteUrl);

                        testOutputHelper().WriteLine("RequestMessage: Body: "+ JsonConvert.SerializeObject(logEntry.RequestMessage.Body));
                    }
                }
            }

        };
        return auth0ServerDependency;
    }

}

public interface ITestOutputHelper
{
    void WriteLine(string message)
    {
        TestContext.Out.WriteLine (message);    Debug.WriteLine(message);
    }
}