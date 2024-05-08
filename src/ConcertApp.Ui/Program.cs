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
builder.Services.ConfigureApplicationCookie(o =>
{
    // Customize the cookie settings here
    o.LoginPath = "/Account/Login"; // Ensure this is the path of your login logic
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.Cookie.HttpOnly = true;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Sets the timeout for the login session
    o.SlidingExpiration = true;
    o.Cookie.SameSite = SameSiteMode.Lax; // Check and adjust according to your application requirements
});
builder.Services.AddIdentityCore<IdentityUser>()
    .AddSignInManager<OpenIdConnectSignInManager>() //Only add the necessary services for authentication
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(o => { o.DefaultScheme = OpenIdConnectDefaults.AuthenticationScheme; })
    .AddOpenIdConnect(o =>
    {
        var appSettings = new OpenIdConnectionAppSettingsOptions();
        builder.Configuration.GetRequiredSection("Authentication:Microsoft").Bind(appSettings);
        appSettings.Validate();

        o.ClientId = appSettings.ClientId;
        o.ClientSecret = appSettings.ClientSecret;
        o.SignInScheme = appSettings.SignInScheme;
        o.Authority = appSettings.Authority;
        o.TokenValidationParameters.ValidIssuer = appSettings.ValidIssuer;
        o.TokenValidationParameters.ValidAudience = appSettings.ValidAudience;
        o.Prompt = appSettings.Prompt;
        o.UsePkce = appSettings.UsePkce!.Value;
        o.ResponseMode = appSettings.ResponseMode;
        o.ResponseType = appSettings.ResponseType;
        o.Scope.Add(appSettings.Scope);
        o.MapInboundClaims = appSettings.MapInboundClaims.GetValueOrDefault();
        o.GetClaimsFromUserInfoEndpoint = appSettings.GetClaimsFromUserInfoEndpoint.GetValueOrDefault();
        o.ClaimActions.MapJsonKey(ClaimTypes.Name, appSettings.ClaimName);
    }).AddIdentityCookies();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make Session cookies essential
});

builder.Services.TryAddScoped<IUserStore<IdentityUser>, InMemoryUserStore>();
builder.Services.TryAddScoped<IRoleStore<IdentityRole>, InMemoryRoleStore>();

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
    "default",
    "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

namespace ConcertApp.Ui
{
    public class Program
    {
    }
}