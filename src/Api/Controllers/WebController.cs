using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class WebController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}