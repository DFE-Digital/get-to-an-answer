using System.Diagnostics;
using System.Net;
using System.Web;
using Common.Models.PageModels;
using Frontend.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Pages;

public class Error(ILogger<Error> logger) : BasePageModel
{
    [BindProperty] public string? ErrorMessage { get; set; }
    [BindProperty] public string? ErrorDescription { get; set;}
    [BindProperty] public string? RequestId { get; set; }
    [BindProperty] public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    
    [BindProperty] public bool IsEmbedded { get; set; }
    
    [FromRoute(Name = "errorCode")] public int? ErrorCode { get; set; }
    
    public void OnGet()
    {
        // TODO: Speak to UCD about how to present this in the error page
        // RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        // Get the original path that caused the error
        var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        var questionnaireRunFeature = HttpContext.Features.Get<QuestionnaireRunFeature>();
        
        if (statusCodeReExecuteFeature != null && !string.IsNullOrWhiteSpace(statusCodeReExecuteFeature.OriginalQueryString))
        {
            IsEmbedded = IsEmbeddedInFrame(statusCodeReExecuteFeature.OriginalQueryString);
        } 
        else if (questionnaireRunFeature != null)
        {
            IsEmbedded = questionnaireRunFeature.IsEmbedded;
        }
        
        if (ErrorCode != null)
        {
            Response.StatusCode = ErrorCode.Value;
            
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
    
    private bool IsEmbeddedInFrame(string? queryString)
    {
       // Parse query string parameters
        if (!string.IsNullOrEmpty(queryString))
        {
            // Remove leading '?' if present
            var cleanQueryString = queryString.TrimStart('?');
            
            // Parse query string into key-value pairs
            var queryCollection = HttpUtility.ParseQueryString(cleanQueryString);

            return queryCollection["embed"]?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        
        return false;
    }
}