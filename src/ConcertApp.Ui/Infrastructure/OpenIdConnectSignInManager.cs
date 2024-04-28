using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ConcertApp.Ui.Infrastructure;


public class OpenIdConnectSignInManager:SignInManager<IdentityUser>
{
    public OpenIdConnectSignInManager(UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<IdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<IdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<IdentityUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        AuthenticationScheme = "AuthenticationTypes.Federation";
    }
    
    public override bool IsSignedIn(ClaimsPrincipal principal)
    {
        if (base.IsSignedIn(principal))
            return true;
        
        return principal?.Identities != null &&
               principal.Identities.Any(i => 
                   i.AuthenticationType == OpenIdConnectDefaults.AuthenticationScheme || 
                   i.AuthenticationType ==  "AuthenticationTypes.Federation" ||
                   i.AuthenticationType == IdentityConstants.ExternalScheme);
    }
}