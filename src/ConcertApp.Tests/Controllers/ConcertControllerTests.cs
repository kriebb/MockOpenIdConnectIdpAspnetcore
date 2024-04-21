using System.Net;
using System.Net.Http.Json;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OIdcMockingInfrastructure.Jwt;
using OIdcMockingInfrastructure.Models;
using OIdcMockingInfrastructure.Security;
using Shouldly;
using Xunit.Abstractions;

namespace ConcertApp.Tests.Controllers;

public class ConcertControllerTests : IClassFixture<ServerSetupFixture>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ServerSetupFixture _fixture;

    public ConcertControllerTests(ITestOutputHelper testOutputHelper, ServerSetupFixture fixture)
    {
        _testOutputHelper = testOutputHelper;
        _fixture = fixture;
    }



    public sealed class PurchaseTicket : ConcertControllerTests,IDisposable
    {
        private const string InvalidIssuer = "SomeOtherIssuer";

        private const string InvalidAudience = "SomeOtherAudience";

       
        private const string ScopeClaimType = "scope";
        private const string ScopeClaimInvalidValue = "users:read";

        private const string CountryClaimType = "country";
        private const string CountryClaimInvalidValue = "France";


        public PurchaseTicket(ITestOutputHelper testOutputHelper, ServerSetupFixture fixture) : base(
            testOutputHelper,
            fixture)
        {
            fixture.SetOutputHelper(testOutputHelper);
            
        }


        [Fact()]
        public async Task WhenPurchaseSeatsForAConcert_WithoutAccessToken_ShouldReturn401()
        {
            var httpClient = _fixture.CreateDefaultClient();
            var response = await httpClient.PostAsync("api/concert/buy", JsonContent.Create(new { NumberOfTickets = 1, ConcertId = 1}));
          
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        private JwtBearerCustomAccessTokenHandler AddTokens(TokenParameters tokenParameters)
        {
            return new JwtBearerCustomAccessTokenHandler(tokenParameters,
                _fixture.Services.GetService<ILogger<ConcertControllerTests>>());
        }

        [Fact()]
        public async Task WhenPurchaseSeatsForAConcert_WithAccessToken_InvalidAudience_ShouldReturn401()
        {

            var tokenParameters = new AccessTokenParameters(InvalidAudience, Consts.ValidIssuer,Consts.ValidSubClaimValue,Consts.ValidScopeClaimValue,Consts.ValidCountryClaimValue);

            var httpClient = _fixture.CreateDefaultClient(AddTokens(tokenParameters));
            


            var response = await httpClient.PostAsync("api/concert/buy", JsonContent.Create(new { NumberOfTickets = 1, ConcertId = 1}));

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

            
        }
        
        [Fact()]
        public async Task WhenPurchaseSeatsForAConcert_WithAccessToken_InvalidIssuer_ShouldReturn401()
        {

            var tokenParameters = new AccessTokenParameters(Consts.ValidAudience, InvalidIssuer,Consts.ValidSubClaimValue,Consts.ValidScopeClaimValue,Consts.ValidCountryClaimValue);
   

            var httpClient = _fixture.CreateDefaultClient(AddTokens(tokenParameters));

            var response = await httpClient.PostAsync("api/concert/buy", JsonContent.Create(new { NumberOfTickets = 1, ConcertId = 1}));

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

            
        }
        
        [Fact()]
        public async Task WhenPurchaseSeatsForAConcert_WithAccessToken_InvalidSignature_ShouldReturn401()
        {
            var tokenParameters = new AccessTokenParameters(Consts.ValidAudience, Consts.ValidIssuer,Consts.ValidSubClaimValue,Consts.ValidScopeClaimValue,Consts.ValidCountryClaimValue)
            {
                SigningCertificate = SelfSignedAccessTokenPemCertificateFactory.Create().ToX509Certificate2()
            };

            var httpClient = _fixture.CreateDefaultClient(AddTokens(tokenParameters));

            var response = await httpClient.PostAsync("api/concert/buy", JsonContent.Create(new { NumberOfTickets = 1, ConcertId = 1}));
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
        
        [Fact()]
        public async Task WhenPurchaseSeatsForAConcert_WithAccessToken_InvalidScope_ShouldReturn403()
        {
            var tokenParameters = new AccessTokenParameters(Consts.ValidAudience, Consts.ValidIssuer,
                Consts.ValidSubClaimValue, Consts.ValidScopeClaimValue, Consts.ValidCountryClaimValue);
            tokenParameters.AddOrReplaceClaim(ScopeClaimType, ScopeClaimInvalidValue);


            var httpClient = _fixture.CreateDefaultClient(AddTokens(tokenParameters));
            var response = await httpClient.PostAsync("api/concert/buy", JsonContent.Create(new { NumberOfTickets = 1, ConcertId = 1}));
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }
        
        [Fact()]
        public async Task WhenPurchaseSeatsForAConcert_WithAccessToken_InvalidClaim_ShouldReturn403()
        {
            var tokenParameters = new AccessTokenParameters(Consts.ValidAudience, Consts.ValidIssuer,
                Consts.ValidSubClaimValue, Consts.ValidScopeClaimValue, Consts.ValidCountryClaimValue);
            tokenParameters.AddOrReplaceClaim(CountryClaimType, CountryClaimInvalidValue);


            var httpClient = _fixture.CreateDefaultClient(AddTokens(tokenParameters));
            var response = await httpClient.PostAsync("api/concert/buy", JsonContent.Create(new { NumberOfTickets = 1, ConcertId = 1}));
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

            
        }
        
        [Fact()]
        public async Task WhenPurchaseSeatsForAConcert_WithValidAccessToken_ShouldReturn200()
        {
            var tokenParameters = new AccessTokenParameters(Consts.ValidAudience, Consts.ValidIssuer,
                Consts.ValidSubClaimValue, Consts.ValidScopeClaimValue, Consts.ValidCountryClaimValue);

           var httpClient = _fixture.CreateDefaultClient(AddTokens(tokenParameters));
           var response = await httpClient.PostAsync("api/concert/buy", JsonContent.Create(new { NumberOfTickets = 1, ConcertId = 1}));

         
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }


        public void Dispose()
        {
            _fixture.ClearOutputHelper();
        }
    }
}