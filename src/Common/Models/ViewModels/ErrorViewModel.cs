using System.Diagnostics.CodeAnalysis;

namespace Common.Models.ViewModels;

[ExcludeFromCodeCoverage(Justification = "Standard ASP.NET Core error model")]
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}