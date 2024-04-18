using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

    //https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins?view=aspnetcore-8.0

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = MicrosoftAccountDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            
        })
        .AddMicrosoftAccount(microsoftOptions =>
        {
            microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ??
                                        throw new ArgumentNullException();
            microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ??
                                            throw new ArgumentNullException();
            microsoftOptions.UsePkce = true;
            microsoftOptions.CallbackPath = "/signin-microsoft";
            
        })
        .AddCookie(setup =>
        {
            setup.LoginPath = "/Account/Login";
            setup.LogoutPath = "/Account/Logout";
        });
    builder.Services.AddHttpContextAccessor();
    builder.Services.TryAddScoped<IUserValidator<IdentityUser>, UserValidator<IdentityUser>>();
    builder.Services.TryAddScoped<IPasswordValidator<IdentityUser>, PasswordValidator<IdentityUser>>();
    builder.Services.TryAddScoped<IPasswordHasher<IdentityUser>, PasswordHasher<IdentityUser>>();
    builder.Services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
    builder.Services.TryAddScoped<IRoleValidator<IdentityRole>, RoleValidator<IdentityRole>>();
    builder.Services.TryAddScoped<IdentityErrorDescriber>();
    builder.Services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<IdentityUser>>();
    builder.Services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<IdentityUser>>();
    builder.Services.TryAddScoped<IUserClaimsPrincipalFactory<IdentityUser>, UserClaimsPrincipalFactory<IdentityUser, IdentityRole>>();
    builder.Services.TryAddScoped<IUserConfirmation<IdentityUser>, DefaultUserConfirmation<IdentityUser>>();
    builder.Services.TryAddScoped<UserManager<IdentityUser>>();
    builder.Services.TryAddScoped<SignInManager<IdentityUser>>();
    builder.Services.TryAddScoped<RoleManager<IdentityRole>>();

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


public partial class Program
{

}
public class InMemoryUserStore : IUserStore<IdentityUser>
{
    private readonly ConcurrentDictionary<string, IdentityUser> _users = new();

    public void Dispose()
    {
        _users.Clear();
    }

    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);

        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);

        _users.TryGetValue(user.Id, out var storedUser);
        return Task.FromResult(storedUser?.UserName);
    }

    public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);

        _users.TryGetValue(user.Id, out var storedUser);
        return Task.FromResult(storedUser?.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);
        return Task.CompletedTask;
    }

    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.TryAdd(user.Id, user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.TryRemove(user.Id, out _);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var user = _users.Values.FirstOrDefault(u => u.NormalizedUserName == normalizedUserName);
        return Task.FromResult(user);
    }
}

public class InMemoryRoleStore : IRoleStore<IdentityRole>
{
    private readonly ConcurrentDictionary<string, IdentityRole> _roles = new();

    public void Dispose()
    {
        _roles.Clear();
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);

        return Task.FromResult(role.Id);
    }

    public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);

        _roles.TryGetValue(role.Id, out var storedRole);
        return Task.FromResult(storedRole?.Name);
    }

    public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);

        _roles.TryGetValue(role.Id, out var storedRole);
        return Task.FromResult(storedRole?.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);
        return Task.CompletedTask;
    }

    public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.TryAdd(role.Id, role);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.TryRemove(role.Id, out _);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        _roles.TryGetValue(roleId, out var role);
        return Task.FromResult(role);
    }

    public Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = _roles.Values.FirstOrDefault(r => r.NormalizedName == normalizedRoleName);
        return Task.FromResult(role);
    }
}