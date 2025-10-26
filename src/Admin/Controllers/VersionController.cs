using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Client;
using Common.Domain;
using Common.Enum;

namespace Admin.Controllers;

using Admin.Models;
using Common.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class VersionController(IApiClient apiClient) : Controller
{
    [HttpGet("admin/questionnaires/{questionnaireId}/versions")]
    public async Task<IActionResult> Compare(Guid questionnaireId)
    {
        // Replace these with calls to your data source
        var vm = new QuestionnaireViewModel
        {
            QuestionnaireId = questionnaireId,
            //CurrentVersionHtml = currentHtml,
            //CurrentVersion = currentVersion.Version,
            //OtherVersionHtml = otherHtml,
            //OtherVersion = versionNumber,
            QuestionnaireVersions = await apiClient.GetQuestionnaireVersionsAsync(questionnaireId)
        };
        return View("QuestionnaireDiff", vm);
    }
}