using Admin.Models;
using Common.Client;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Admin.Pages.Answers;

public class EditAnswerOptions(ILogger<EditAnswerOptions> logger, IApiClient apiClient) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty] public string? QuestionNumber { get; set; } = "1";

    [BindProperty] public List<AnswerOptionsViewModel> Options { get; set; } = [];

    [BindProperty] public QuestionType? RetrievedQuestionType { get; set; }


    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.EditQuestion, QuestionnaireId, QuestionId);

        if (TempData.TryGetValue("QuestionType", out var rawValue)
            && rawValue is int intVal
            && Enum.IsDefined(typeof(QuestionType), intVal))
        {
            RetrievedQuestionType = (QuestionType)intVal;
        }

        // await PopulateFieldOptions();
        await PopulateFieldWithExistingValues();

        ReassignOptionNumbers();

        return Page();
    }


    private async Task PopulateFieldWithExistingValues()
    {
        var exitingAnswers = await apiClient.GetAnswersAsync(QuestionId);
        var questionForSelection = await apiClient.GetQuestionsAsync(QuestionnaireId);
        var currentQuestion = questionForSelection.SingleOrDefault(q => q.Id == QuestionId);
        
        
        var questionSelectionList = questionForSelection.Where(x => x.Id != QuestionId)
            .Select(q => new SelectListItem(q.Content, q.Id.ToString())).ToList();

        // Set the selected property on the question selection list            
        questionSelectionList.Select(x => x.Selected = exitingAnswers.Any(a => a.Id.ToString() == x.Value));

        foreach (var exitingAnswer in exitingAnswers)
        {
            Options.Add(new AnswerOptionsViewModel
            {
                QuestionSelectList = questionSelectionList,
                AnswerDestination = MapAnswerDestination(exitingAnswer.DestinationType),
                OptionContent = exitingAnswer.Content,
                OptionHint = exitingAnswer.Description,
                ExternalLink = exitingAnswer.DestinationUrl,
                SelectedDestinationQuestion = exitingAnswer.DestinationQuestionId?.ToString(),
                OptionNumber = exitingAnswers.Count - 1,
                QuestionType = currentQuestion?.Type,
                RankPriority = exitingAnswer.Priority.ToString(),
                ResultPageUrl = exitingAnswer.DestinationUrl,
                //ResultsPageSelectList =
                //SelectedResultsPage = 
            });
        }

        // foreach (var option in Options)
        // {
        //     option.QuestionSelectList.Select(x => x.Selected = exitingAnswers.Any(a => a.Id.ToString() == x.Value));
        // }
    }

    private async Task PopulateFieldOptions()
    {
        var questions = await apiClient.GetQuestionsAsync(QuestionnaireId);
        // var resultsPages = await apiClient.GetResultsPagesAsync(QuestionnaireId);

        var questionSelect = questions.Where(x => x.Id != QuestionId)
            .Select(q => new SelectListItem(q.Content, q.Id.ToString())).ToList();
        // var resultsSelect = resultsPages.Select(r => new SelectListItem(r.Title, r.Id.ToString())).ToList();

        foreach (var option in Options)
        {
            option.QuestionType = RetrievedQuestionType;
            option.QuestionSelectList = questionSelect;
            // option.ResultsPageSelectList = resultsSelect;
        }
    }

    private void ReassignOptionNumbers()
    {
        for (var index = 0; index < Options.Count; index++)
        {
            Options[index].OptionNumber = index + 1;
        }
    }
    
    private static AnswerDestination MapAnswerDestination(DestinationType? destinationType) =>
        destinationType switch
        {
            DestinationType.Question => AnswerDestination.NextQuestion,
            DestinationType.CustomContent => AnswerDestination.InternalResultsPage,
            DestinationType.ExternalLink => AnswerDestination.ExternalResultsPage,
            _ => throw new ArgumentOutOfRangeException(nameof(destinationType), destinationType, null)
        };
}