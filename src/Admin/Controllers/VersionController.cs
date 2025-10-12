using System.Text.Json;
using Admin.Client;
using Common.Domain;

namespace Admin.Controllers;

using Admin.Models;
using Common.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class VersionController(IApiClient apiClient) : Controller
{
    [HttpGet("admin/questionnaires/{questionnaireId}/versions/{versionNumber}/compare")]
    public async Task<IActionResult> Compare(int questionnaireId, int versionNumber)
    {
        // Replace these with calls to your data source
        var currentVersion = await apiClient.GetLatestQuestionnaireVersion(questionnaireId);
        var otherVersion = await apiClient.GetQuestionnaireVersionAsync(questionnaireId, versionNumber);
        
        if (currentVersion == null || otherVersion == null)
            return NotFound();

        var (otherHtml, currentHtml) = VersionDiffRenderer.RenderCompare(otherVersion?.ToJson(), currentVersion.ToJson());

        var vm = new QuestionnaireViewModel
        {
            CurrentVersionHtml = currentHtml,
            CurrentVersion = currentVersion.Version,
            OtherVersionHtml = otherHtml,
            OtherVersion = versionNumber,
            QuestionnaireVersions = await apiClient.GetQuestionnaireVersionsAsync(questionnaireId)
        };
        return View("QuestionnaireDiff", vm);
    }
}

public static class QuestionnaireDtoExtensions
{
    public static string ToJson(this QuestionnaireDto questionnaire)
    {
        return JsonSerializer.Serialize(questionnaire);
    }
}