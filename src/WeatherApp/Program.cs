using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Internal;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddHttpLogging(options => options.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.ResponseBody);


//TODO: 04_Explain ensure that there is a jwt attribute
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    
}).AddJwtBearer();



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

/**********************************************/
//TODO: 01_01_First UseAuthentication!
app.UseAuthentication();
//TODO: 01_02_Explain Second UseAuthorization!
app.UseAuthorization();
/**********************************************/

app.MapControllers();


app.Run();


#pragma warning disable CA1050
//TODO: 01_a_Explain_WebApplicationFactory Program is internal, and needed to refer to the tests, so set it public using partial class
//InternalsVisibleTo does not seem to work
public partial class Program
#pragma warning restore CA1050
{
}

