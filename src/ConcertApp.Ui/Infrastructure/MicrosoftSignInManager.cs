using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ConcertApp.Ui.Infrastructure;

public class MicrosoftSignInManager:SignInManager<IdentityUser>
{
    public MicrosoftSignInManager(UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<IdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<IdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<IdentityUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        AuthenticationScheme = MicrosoftAccountDefaults.AuthenticationScheme;
    }
}