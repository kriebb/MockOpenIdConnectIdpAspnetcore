using System.Diagnostics;
using ConcertApp.Ui.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConcertApp.Ui.Controllers;


public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(ILogger<AccountController> logger, SignInManager<IdentityUser> signInManager)
    {
        _logger = logger;
        _signInManager = signInManager;
    }

    public IActionResult Index()
    {
        var model = new ManageAccountViewModel (IsAuthenticated: User.Identity is { IsAuthenticated: true }, Username: User.Identity?.Name,
            AuthenticationType: User.Identity?.AuthenticationType, Claims: User.Claims.Select(c => new ClaimViewModel
            (
            
                
                Type : c.Type,
                Value : c.Value
            )).ToList());

        return View("Manage/Index",model);    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);       
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return RedirectToAction("LoggedOut");
    }
    [AllowAnonymous]
    public async Task<IActionResult> LoggedOut()
    {
        return View("Loggedout");
    }
    public async Task<IActionResult> Login()
    {
        return View("Login", new LoginViewModel(HttpContext.User.Identity.Name));
    }
}