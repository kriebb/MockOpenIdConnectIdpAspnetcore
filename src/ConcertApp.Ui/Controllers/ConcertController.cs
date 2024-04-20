using System.Diagnostics;
using ConcertApp.Ui.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConcertApp.Ui.Controllers;


public class ConcertController : Controller
{
    private readonly ILogger<ConcertController> _logger;

    public ConcertController(ILogger<ConcertController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var model = new ConvertViewModel(new List<ConvertViewModel.ConcertItem>());

        return View(model);    
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}