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
    [HttpGet("admin/questionnaires/{questionnaireId}/versions/{versionNumber}/compare")]
    public async Task<IActionResult> Compare(int questionnaireId, int versionNumber)
    {
        // Replace these with calls to your data source
        var currentVersion = await apiClient.GetLatestQuestionnaireVersion(questionnaireId);
        var otherVersion = await apiClient.GetQuestionnaireVersionAsync(questionnaireId, versionNumber);
        
        if (currentVersion == null || otherVersion == null)
            return NotFound();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        
        static string LimitToQuestionnaireContentJson(string? json, JsonSerializerOptions opts)
        {
            if (json == null)
                return string.Empty;
            
            // Deserialize to the known type (ignores unknown fields) and serialize back.
            // Replace with a custom Utf8JsonReader/Writer filter if you want pure streaming.
            var model = JsonSerializer.Deserialize<QuestionnaireContent>(json, opts);
            return JsonSerializer.Serialize(model, opts);
        }

        var otherJson = LimitToQuestionnaireContentJson(otherVersion.QuestionnaireJson, options);
        var currentJson = LimitToQuestionnaireContentJson(currentVersion.QuestionnaireJson, options);

        var (otherHtml, currentHtml) = VersionDiffRenderer.RenderCompare(otherJson, currentJson);

        var vm = new QuestionnaireViewModel
        {
            QuestionnaireId = questionnaireId,
            CurrentVersionHtml = currentHtml,
            CurrentVersion = currentVersion.Version,
            OtherVersionHtml = otherHtml,
            OtherVersion = versionNumber,
            QuestionnaireVersions = await apiClient.GetQuestionnaireVersionsAsync(questionnaireId)
        };
        return View("QuestionnaireDiff", vm);
    }
}

public class QuestionnaireContent
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public EntityStatus? Status { get; set; }
    
    public List<QuestionContent>? Questions { get; set; }
}

public class QuestionContent
{
    public string? Content { get; set; }
    public string? Description { get; set; }
    public int? Order { get; set; }
    public EntityStatus Status { get; set; }
    public QuestionType Type { get; set; }
    
    public List<AnswerContent>? Answers { get; set; }
}

public class AnswerContent
{
    public string? Content { get; set; }
    public string? Description { get; set; }
    public DestinationType? DestinationType { get; set; }
    
    public string? Destination { get; set; }
    
    public int? DestinationQuestionId { get; set; }
    
    public int? DestinationContentId { get; set; }
}