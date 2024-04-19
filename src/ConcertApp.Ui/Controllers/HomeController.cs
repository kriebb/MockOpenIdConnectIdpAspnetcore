using System.Diagnostics;
using ConcertApp.Ui.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConcertApp.Ui.Controllers;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new IndexViewModel(IsAuthenticated: User.Identity is { IsAuthenticated: true }, Username: User.Identity?.Name,
            AuthenticationType: User.Identity?.AuthenticationType, Claims: User.Claims.Select(c => new ClaimViewModel
            (
            
                
                Type : c.Type,
                Value : c.Value
            )).ToList());

        return View(model);    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}