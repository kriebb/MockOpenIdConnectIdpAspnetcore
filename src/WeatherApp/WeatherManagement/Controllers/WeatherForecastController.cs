using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;
using WeatherApp.WeatherManagement.Controllers.Models;
using WeatherApp.WeatherManagement.Services.OpenMeteo.Models;

namespace WeatherApp.WeatherManagement.Controllers;

[ApiController]
[Route("[controller]")]
//Demo 2 09_AuthorizeBelgium
[Authorize]
public class WeatherForecastController(ISystemClock systemClock) : ControllerBase
{
    private readonly ISystemClock _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));

    private static readonly TemperatureCelciusBucket[] TempCBuckets = new[]
    {
        new TemperatureCelciusBucket(new TempCRange(100,double.MaxValue),"WTF burning"),

        new TemperatureCelciusBucket(new TempCRange(29,100),"Too hot"),
        new TemperatureCelciusBucket(new TempCRange(21,28),"Cozy"),
        new TemperatureCelciusBucket(new TempCRange(15,20),"Cold"),
        new TemperatureCelciusBucket(new TempCRange(-100,15),"Too cold"),
        new TemperatureCelciusBucket(new TempCRange(double.MinValue,-99),"WTF freezing"),

    };


    [HttpGet()]
    //Demo 2 10_AuathorizeGetOperation
    [Authorize]
    public WeatherForecast Get()
    {
        // Generate random bytes from the RandomNumberGenerator

        byte[] bytes = RandomNumberGenerator.GetBytes(8);

        // Convert the byte array to a double
        double randomTemp = BitConverter.ToDouble(bytes, 0);
        var bucket = TempCBuckets.Single(tempCBucket =>
            randomTemp >= tempCBucket.minMaxTempC.Min &&
            randomTemp < tempCBucket.minMaxTempC.Max);


        return new WeatherForecast()
        {
            Date = _systemClock.UtcNow.Date,
            Summary = bucket.Name,
            TemperatureC = randomTemp
        };
    }
}