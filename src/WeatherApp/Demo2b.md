We are going to secure our server.

- - WeatherForecastController.cs

  - Class level 
[Authorize(Policy = "OnlyBelgium")]
  - Operation level
[Authorize(Policy = "WeatherForecast:Get")]


Program.s
builder.Services.AddAuthorization(authorizationOptions =>
{

    authorizationOptions.AddPolicy("OnlyBelgium", policy =>
    {
        policy.RequireClaim("country", "Belgium");

    });

    authorizationOptions.AddPolicy("WeatherForecast:Get", policy =>
    {
        policy.RequireClaim("scope", "weatherforecast:read");
    });
});


Go to the test and run again.
/////////////////////////////////////////////////////////////////////////////////////////