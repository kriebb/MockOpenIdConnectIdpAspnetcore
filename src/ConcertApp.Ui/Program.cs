using ConcertApp.Ui;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddDebug();
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
builder.Services.AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddSignInManager<MicrosoftSignInManager>()  // Only add the necessary services for authentication
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        // use the default scheme for everything unless specified otherwise
        options.DefaultScheme = MicrosoftAccountDefaults.AuthenticationScheme;
    })
    .AddMicrosoftAccount(microsoftOptions =>
    {
        microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
        microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
        microsoftOptions.SignInScheme = IdentityConstants.ExternalScheme;
    }).AddIdentityCookies();


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

public class MicrosoftSignInManager:SignInManager<IdentityUser>
{
    public MicrosoftSignInManager(UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<IdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<IdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<IdentityUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        AuthenticationScheme = MicrosoftAccountDefaults.AuthenticationScheme;
    }
}

namespace ConcertApp.Ui
{
    public partial class Program
    {

    }
}