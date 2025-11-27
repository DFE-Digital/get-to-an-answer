using System.ComponentModel.DataAnnotations;
using Common.Validation;

namespace Common.Domain.Request.Update;

public class UpdateLookAndFeelRequestDto
{
    [GdsColor] public string? TextColor { get; set; } = "#0b0c0c";
    [GdsColor] public string? BackgroundColor { get; set; } = "#ffffff";
    [GdsColor] public string? PrimaryButtonColor { get; set; } = "#00703c";
    [GdsColor] public string? SecondaryButtonColor { get; set; } = "#1d70b8";
    [GdsColor] public string? StateColor { get; set; } = "#ffdd00";
    [GdsColor] public string? ErrorMessageColor { get; set; } = "#c3432b";
    
    [MaxLength(250)] public string? DecorativeImage { get; set; }
    
    public bool IsAccessibilityAgreementAccepted { get; set; } = true;
}