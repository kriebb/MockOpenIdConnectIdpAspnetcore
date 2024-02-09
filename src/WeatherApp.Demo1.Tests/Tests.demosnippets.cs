//DEMOSNIPPETS-TAB Demo5 Application
//DEMOSNIPPETS-LABEL 01_ServerSetupFixture_CreateHostWithWireMockOIDC
protected override IHost CreateHost(IHostBuilder builder)
{

    //HttpClient.DefaultProxy = new WebProxy(new Uri("http://localhost:8888"));
    var wireMockServer = WireMockServer.Start(new WireMockServerSettings()
    {
        Urls = new[] { "https://localhost:6666" },
        SaveUnmatchedRequests = true,
        StartAdminInterface = true,

    });

    wireMockServer
        .Given(Request.Create().WithPath("/.well-known/openid-configuration")
            .UsingGet())
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBodyAsJson(Consts.ValidOpenIdConnectDiscoveryDocumentConfiguration));

    // Configure stub for JWKS URI
    wireMockServer
        .Given(Request.Create().WithPath("/.well-known/jwks").UsingGet())
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBodyAsJson(

                Consts.ValidSigningCertificate.ToJwksCertificate()));


    return base.CreateHost(builder);
}

//DEMOSNIPPETS-LABEL 03_ServerSetupFixture_RemoveConfig
//Remove assignment
//Remove class ConfigForMockedOpenIdConnectServer.cs
//Remove class MockingOpenIdProviderMessageHandler.cs

//DEMOSNIPPETS-LABEL 02_ServerSetupFixture_AddUrl
configuration.AddInMemoryCollection(new KeyValuePair<string, string?>[]
{
    new("Jwt:MetadataAddress", Consts.WellKnownOpenIdConfiguration)
});