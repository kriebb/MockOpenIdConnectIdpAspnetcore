using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Internal;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddHttpLogging(options => options.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.ResponseBody);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    
})//DEMO2 REPLACE BELOW
    .AddJwtBearer(o =>
    {
        o.MetadataAddress = builder.Configuration["Jwt:MetadataAddress"];
    o.MapInboundClaims = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        NameClaimType = "sub",

    };
});

//DEMO2 INSERT BELOW
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
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.UseHttpLogging();

}
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();


#pragma warning disable CA1050
//Autogenerated Program class is internal, and needed to refer to the tests, so set it public using partial class
//InternalsVisibleTo does not seem to work
public partial class Program
#pragma warning restore CA1050
{
}

