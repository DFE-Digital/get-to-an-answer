using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Checker.Admin.Models;

namespace Checker.Admin.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(new ConfigViewModel());
    }

    public IActionResult Privacy()
    {
        return View(new ConfigViewModel());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}