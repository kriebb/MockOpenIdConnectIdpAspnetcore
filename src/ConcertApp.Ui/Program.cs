using System.Security.Claims;
using ConcertApp.Ui.Infrastructure;
using ConcertApp.Ui.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddDebug();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Configuration.AddUserSecrets<ConcertApp.Ui.Program>();

//https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins?view=aspnetcore-8.0
builder.Services.ConfigureApplicationCookie(options =>
{
    // Customize the cookie settings here
    options.LoginPath = "/Account/Login"; // Ensure this is the path of your login logic
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Sets the timeout for the login session
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Check and adjust according to your application requirements
});
builder.Services.AddIdentityCore<IdentityUser>()
    .AddSignInManager<OpenIdConnectSignInManager>() //Only add the necessary services for authentication
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddOpenIdConnect(aspnetCoreOidcOptions =>
    {
        var appSettingsOptions = new OpenIdConnectionAppSettingsOptions();
        builder.Configuration.GetRequiredSection("Authentication:Microsoft").Bind(appSettingsOptions);
        appSettingsOptions.Validate();
        
        aspnetCoreOidcOptions.ClientId = appSettingsOptions.ClientId;
        aspnetCoreOidcOptions.ClientSecret = appSettingsOptions.ClientSecret;
        aspnetCoreOidcOptions.SignInScheme = appSettingsOptions.SignInScheme;
        aspnetCoreOidcOptions.Authority = appSettingsOptions.Authority;
        aspnetCoreOidcOptions.TokenValidationParameters.ValidIssuer = appSettingsOptions.ValidIssuer;
        aspnetCoreOidcOptions.TokenValidationParameters.ValidAudience = appSettingsOptions.ValidAudience;
        aspnetCoreOidcOptions.Prompt = appSettingsOptions.Prompt;
        aspnetCoreOidcOptions.UsePkce = appSettingsOptions.UsePkce!.Value;
        aspnetCoreOidcOptions.ResponseMode = appSettingsOptions.ResponseMode;
        aspnetCoreOidcOptions.ResponseType = appSettingsOptions.ResponseType;
        aspnetCoreOidcOptions.Scope.Add(appSettingsOptions.Scope);
        aspnetCoreOidcOptions.MapInboundClaims = appSettingsOptions.MapInboundClaims.GetValueOrDefault();
        aspnetCoreOidcOptions.GetClaimsFromUserInfoEndpoint = appSettingsOptions.GetClaimsFromUserInfoEndpoint.GetValueOrDefault();
        aspnetCoreOidcOptions.ClaimActions.MapJsonKey(ClaimTypes.Name, appSettingsOptions.ClaimName);
        
        
    }).
    AddIdentityCookies();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make Session cookies essential
});

builder.Services.TryAddScoped<IUserStore<IdentityUser>,InMemoryUserStore>();
builder.Services.TryAddScoped<IRoleStore<IdentityRole>,InMemoryRoleStore>();

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews(options =>
{
    
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddRazorPages().AddMvcOptions(x => { });
var app = builder.Build();
app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

namespace ConcertApp.Ui
{
    public partial class Program
    {

    }
}