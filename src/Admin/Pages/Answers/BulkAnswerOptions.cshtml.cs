using System.Text.RegularExpressions;
using Admin.Models;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Admin.Pages.Answers;

public partial class BulkAnswerOptions(ILogger<AddAnswerOptions> logger) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty] public string? QuestionNumber { get; set; } = "1";

    [BindProperty] public string? BulkAnswerOptionsRawText { get; set; }

    public IActionResult OnPost(string? returnUrl)
    {
        var bulkOptions = SplitOnNewLineRegex().Split(BulkAnswerOptionsRawText ?? string.Empty)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.TrimEnd())
            .ToList();

        var existingOptionsSnapshot = TempData.Peek(AnswerOptionsViewModel.AnswersSnapshotTempDataKey);
        var existingAnswerOptions =
            JsonConvert.DeserializeObject<List<AnswerOptionsViewModel>>(existingOptionsSnapshot?.ToString() ?? "[]");

        if (existingAnswerOptions != null)
        {
            var bulkOptionsAsAnswers = bulkOptions.Select(optionStringValue =>
                new AnswerOptionsViewModel { OptionContent = optionStringValue, });

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

    [GeneratedRegex(@"\s*$\r?\n\s*", RegexOptions.Multiline)]
    private static partial Regex SplitOnNewLineRegex();
}