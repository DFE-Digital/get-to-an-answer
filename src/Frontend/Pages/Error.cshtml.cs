using System.Net;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Pages;

public class Error(ILogger<Error> logger) : BasePageModel
{
    [BindProperty] public string? ErrorMessage { get; set; }
    [BindProperty] public string? ErrorDescription { get; set;}
    
    [FromRoute(Name = "errorCode")] public int? ErrorCode { get; set; }
    
    public void OnGet()
    {
        if (ErrorCode != null)
        {
            ErrorMessage = ErrorCode switch
            {
                (int) HttpStatusCode.NotFound => "Page not found.",
                (int) HttpStatusCode.Forbidden => "Access denied.",
                (int) HttpStatusCode.BadRequest => "Bad request.",
                (int) HttpStatusCode.InternalServerError => "Internal server error.",
                (int) HttpStatusCode.ServiceUnavailable => "Service unavailable.",
                _ => "An error occurred."
            };
        }
        else
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            
            ErrorCode = (int) HttpStatusCode.InternalServerError;
            ErrorMessage = "Internal server error.";

            // Log the error (you can use Serilog, NLog, etc.)
            if (exceptionHandlerPathFeature != null)
            {
                Console.WriteLine($"Error Path: {exceptionHandlerPathFeature.Path}");
                Console.WriteLine($"Exception: {exceptionHandlerPathFeature.Error}");
            }
        }
    }
}