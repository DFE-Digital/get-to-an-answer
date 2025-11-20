using Common.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Admin.Models;

public class AnswerOptionsViewModel
{
    public string OptionContent { get; set; } = string.Empty;
    
    public string OptionHint { get; set; } = string.Empty;
    public AnswerDestination AnswerDestination { get; set; }
    
    public IEnumerable<SelectListItem> QuestionSelectList { get; set; } = [];
    public IEnumerable<SelectListItem> ResultsPageSelectList { get; set; } = [];
    
    public string? ResultPageUrl { get; set; }
    
    public string? SelectedResultsPage { get; set; }
    
    public string? SelectedQuestion { get; set; }

    public string? ExternalLink { get; set; }
}