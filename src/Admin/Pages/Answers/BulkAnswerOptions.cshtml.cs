using System.Text.RegularExpressions;
using Admin.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Answers;

public partial class BulkAnswerOptions(ILogger<BulkAnswerOptions> logger) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty] public string? QuestionNumber { get; set; } = "1";

    [BindProperty] public string? BulkAnswerOptionsRawText { get; set; }

    public IActionResult OnPost(string? returnUrl)
    {
        try
        {
            logger.LogInformation("Bulk answer options submitted");

            var bulkOptions = SplitOnNewLineRegex().Split(BulkAnswerOptionsRawText ?? string.Empty)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.TrimEnd())
                .ToList();

            var existingOptionsSnapshot = TempData.Peek(AnswerOptionsViewModel.AnswersSnapshotTempDataKey);
            var existingAnswerOptions =
                JsonConvert.DeserializeObject<List<AnswerOptionsViewModel>>(existingOptionsSnapshot?.ToString() ?? "[]");

            if (existingAnswerOptions != null)
            {
                var bulkOptionsAsAnswers = bulkOptions.Select(optionStringValue => new AnswerOptionsViewModel
                {
                    OptionContent = optionStringValue,
                    AnswerDestination = AnswerDestination.NextQuestion
                });

                existingAnswerOptions.AddRange(bulkOptionsAsAnswers);
            }

            TempData[AnswerOptionsViewModel.AnswersSnapshotTempDataKey] =
                JsonConvert.SerializeObject(existingAnswerOptions);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Page();
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error creating answer options for question {QuestionId} from questionnaire {QuestionnaireId}",
                QuestionId, QuestionnaireId);
            return BadRequest();
        }
    }

    [GeneratedRegex(@"\s*$\r?\n\s*", RegexOptions.Multiline)]
    private static partial Regex SplitOnNewLineRegex();
}