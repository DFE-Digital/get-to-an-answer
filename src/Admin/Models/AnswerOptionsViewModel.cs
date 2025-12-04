using Admin.Attributes;
using Common.Domain;
using Common.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Admin.Models;

public class AnswerOptionsViewModel
{
    public Guid AnswerId { get; set; }
    
    public int OptionNumber { get; set; }
    
    public QuestionType? QuestionType { get; set; }
    
    [AnswerOptionRequired(fieldType : "content")]
    public string? OptionContent { get; set; } = string.Empty;
    
    public string? OptionHint { get; set; } = string.Empty;

    [AnswerOptionRequired(fieldType: "destination")]
    public AnswerDestination AnswerDestination { get; set; } = AnswerDestination.NextQuestion;
    
    public IEnumerable<SelectListItem> QuestionSelectList { get; set; } = [];
    public List<SelectListItem> ResultsPageSelectList { get; set; } = [];
    
    public string? SelectedResultsPage { get; set; }
    
    public string? SelectedDestinationQuestion { get; set; }

    public string? ExternalLink { get; set; }
    
    public string? RankPriority { get; set; }
}