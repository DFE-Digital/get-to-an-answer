using Microsoft.AspNetCore.Mvc;

namespace Checker.Api.Controllers;

public class WebController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}