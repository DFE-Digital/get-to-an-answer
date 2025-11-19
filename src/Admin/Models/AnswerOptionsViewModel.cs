namespace Admin.Models;

public class AnswerOptionsViewModel
{
    public string OptionContent { get; set; } = string.Empty;
    
    public string OptionHint { get; set; } = string.Empty;
    public AnswerOptionDestination Destination { get; set; }
}