using System.ComponentModel.DataAnnotations;
using Common.Validation;

namespace Common.Domain.Request.Update;

public class UpdateLookAndFeelRequestDto
{
    [RegularExpression("[#][0-9a-fA-F]{3}([0-9a-fA-F]{3})?", 
        ErrorMessage = "Color must be a valid hex code")]
    [MaxLength(7)]
    public string? Title { get; set; }
    
    public string TextColor { get; set; } = "#0b0c0c";
    public string BackgroundColor { get; set; } = "#ffffff";
    public string PrimaryButtonColor { get; set; } = "#00703c";
    public string SecondaryButtonColor { get; set; } = "#1d70b8";
    public string StateColor { get; set; } = "#ffdd00";
    public string ErrorColor { get; set; } = "#c3432b";
    
    public string? DecorativeImage { get; set; }

    public bool IsAccessibilityAgreementAccepted { get; set; }
}