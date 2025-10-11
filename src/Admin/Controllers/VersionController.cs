using System.Text.Json;
using Admin.Client;

namespace Admin.Controllers;

using Admin.Models;
using Common.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class VersionController(IApiClient apiClient) : Controller
{
    [HttpGet("admin/questionnaires/{leftId}/compare/{rightId}")]
    public async Task<IActionResult> Compare(int leftId, int rightId)
    {
        // Replace these with calls to your data source
        var oldJson = await GetQuestionnaireJsonAsync(leftId);
        var newJson = await GetQuestionnaireJsonAsync(rightId);

        var (oldHtml, newHtml) = VersionDiffRenderer.RenderCompare(oldJson, newJson);

        var vm = new QuestionnaireViewModel
        {
            OldHtml = oldHtml,
            NewHtml = newHtml
        };
        return View("QuestionnaireDiff", vm);
    }

    private async Task<string> GetQuestionnaireJsonAsync(int id)
    {
        var questionnaire = await apiClient.GetQuestionnaireAsync(id);
        
        return JsonSerializer.Serialize(questionnaire);
    }
}