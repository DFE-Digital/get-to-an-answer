using System.Text.RegularExpressions;
using Common.Client;
using Common.Domain.Request.Create;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Answers;

[Authorize]
public partial class BulkAnswerOptions(IApiClient apiClient, ILogger<BulkAnswerOptions> logger) : BasePageModel
{
    [FromRoute(Name = "questionnaireId")] public Guid QuestionnaireId { get; set; }

    [FromRoute(Name = "questionId")] public Guid QuestionId { get; set; }

    [BindProperty] public string? QuestionNumber { get; set; } = "1";

    [BindProperty] public string? BulkAnswerOptionsRawText { get; set; }
    
    [BindProperty(SupportsGet = true, Name = "returnUrl")]
    public string? ReturnUrl { get; set; }
    
    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        ReturnUrl = returnUrl;
        BackLinkSlug = returnUrl ?? Routes.QuestionnairesManage;

        if (!ModelState.IsValid)
            return Page();

        var answers = await apiClient.GetAnswersAsync(QuestionId);

        if (answers.Count > 0)
        {
            BulkAnswerOptionsRawText = answers.Select(a => a.Content)
                .Aggregate((a, b) => a + "\n" + b).Trim();
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPost(string? returnUrl)
    {
        try
        {
            logger.LogInformation("Bulk answer options submitted");

            var bulkOptions = SplitOnNewLineRegex().Split(BulkAnswerOptionsRawText ?? string.Empty)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.TrimEnd())
                .ToList();

            // entries should be unique, if not, throw an error
            if (bulkOptions.Count != bulkOptions.Distinct().Count())
            {
                ModelState.AddModelError("BulkAnswerOptionsRawText", "Duplicate entries found");

                return Page();
            }

            var answers = await apiClient.GetAnswersAsync(QuestionId);
            var answerMap = answers.ToDictionary(a => a.Content, a => a);

            var bulkUpserts = new BulkUpsertAnswersRequestDto
            {
                QuestionnaireId = QuestionnaireId,
                QuestionId = QuestionId,
            };

            // upsert any new answers 
            foreach (var option in bulkOptions)
            {
                if (answerMap.TryGetValue(option, out var existingAnswer))
                {
                    bulkUpserts.Upserts.Add(new UpsertAnswerRequestDto
                    {
                        Id = existingAnswer.Id,
                        Content = existingAnswer.Content,
                        Description = existingAnswer.Description,
                        DestinationType = existingAnswer.DestinationType,
                        DestinationQuestionId = existingAnswer.DestinationQuestionId,
                        DestinationContentId = existingAnswer.DestinationContentId,
                        DestinationUrl = existingAnswer.DestinationUrl,
                        Priority = existingAnswer.Priority,
                    });
                }
                else
                {
                    bulkUpserts.Upserts.Add(new UpsertAnswerRequestDto
                    {
                        Content = option,
                        DestinationType = DestinationType.Auto
                    });
                }
            }

            await apiClient.BulkUpsertAnswersAsync(bulkUpserts);

            var answerContentList = answers.Select(a => a.Content).ToList();
            var removedAnswers = answerContentList.Except(bulkOptions).ToList();

            // remove any answers that are no longer in the bulk options
            foreach (var option in removedAnswers)
            {
                if (answerMap.TryGetValue(option, out var existingAnswer))
                {
                    await apiClient.DeleteAnswerAsync(existingAnswer.Id);
                }
            }
            
            var target = ReturnUrl ?? returnUrl;
            if (!string.IsNullOrEmpty(target) && Url.IsLocalUrl(target))
            {
                return Redirect(target);
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