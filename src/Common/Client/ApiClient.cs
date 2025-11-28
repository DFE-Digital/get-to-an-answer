using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Domain;
using Common.Domain.Admin;
using Common.Domain.Frontend;
using Common.Domain.Request.Add;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Common.Enum;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Common.Client;

public interface IApiClient
{
    // === For Questionnaires ===
    Task<QuestionnaireDto?> GetQuestionnaireAsync(Guid questionnaireId);
    Task<QuestionnaireInfoDto?> GetLastPublishedQuestionnaireInfoAsync(string questionnaireSlug);
    Task<List<QuestionnaireDto>> GetQuestionnairesAsync();
    Task<QuestionnaireDto?> CreateQuestionnaireAsync(CreateQuestionnaireRequestDto request);
    Task<string?> UpdateQuestionnaireAsync(Guid questionnaireId, UpdateQuestionnaireRequestDto request);
    Task<string?> UpdateQuestionnaireLookAndFeelAsync(Guid questionnaireId, UpdateLookAndFeelRequestDto request);
    Task<string?> UpdateContinueButtonAsync(Guid questionnaireId, UpdateContinueButtonRequestDto request);
    Task<string?> DeleteQuestionnaireDecorativeImageAsync(Guid questionnaireId);
    Task<string?> PublishQuestionnaireAsync(Guid questionnaireId);
    Task<string?> UnpublishQuestionnaireAsync(Guid questionnaireId);
    Task<QuestionnaireDto?> CloneQuestionnaireAsync(Guid questionnaireId, CloneQuestionnaireRequestDto request);
    Task<string?> DeleteQuestionnaireAsync(Guid questionnaireId);
    Task<string?> AddQuestionnaireContributor(Guid questionnaireId, AddContributorRequestDto request);
    Task<string[]> GetQuestionnaireContributors(Guid questionnaireId);
    Task<string?> RemoveQuestionnaireContributor(Guid questionnaireId, string contributorEmail);

    // === For Questions ===

    Task<QuestionDto?> GetQuestionAsync(Guid questionId);
    Task<List<QuestionDto>> GetQuestionsAsync(Guid questionnaireId);
    Task<QuestionDto?> CreateQuestionAsync(CreateQuestionRequestDto request);
    Task<string?> UpdateQuestionAsync(Guid questionId, UpdateQuestionRequestDto request);
    Task<string?> UpdateQuestionStatusAsync(Guid questionId, UpdateQuestionStatusRequestDto request);
    Task<string?> DeleteQuestionAsync(Guid questionId);
    Task<string?> MoveQuestionDownOneAsync(Guid questionnaireId, Guid questionId);
    Task<string?> MoveQuestionUpOneAsync(Guid questionnaireId, Guid questionId);

    // === For Answers ===

    Task<AnswerDto?> GetAnswerAsync(Guid answerId);
    Task<List<AnswerDto>> GetAnswersAsync(Guid questionId);
    Task<AnswerDto?> CreateAnswerAsync(CreateAnswerRequestDto request);
    Task<string?> UpdateAnswerAsync(Guid answerId, UpdateAnswerRequestDto request);
    Task<string?> DeleteAnswerAsync(Guid answerId);

    // === For Questionnaire Versions ===

    Task<List<QuestionnaireVersionDto>> GetQuestionnaireVersionsAsync(Guid questionnaireId);
    Task<QuestionnaireBranchingMapDto?> GetBranchingMap(Guid questionnaireId);

    // === For Content ===

    Task<ContentDto?> CreateContentAsync(CreateContentRequestDto request);
    Task<string?> UpdateContentAsync(Guid questionnaireId, UpdateContentRequestDto request);
    Task<string?> DeleteContentAsync(Guid contentId);
    Task<ContentDto?> GetContentAsync(Guid contentId);
    Task<List<ContentDto>> GetContentsAsync(Guid questionnaireId);

    // === For Service Users ===

    Task<QuestionDto?> GetInitialQuestion(Guid questionnaireId, bool preview = false);
    Task<DestinationDto?> GetNextState(Guid questionnaireId, GetNextStateRequest request, bool preview = false);

    // === For Inviting Contributors ===

    Task<string?> AddSelfToQuestionnaireContributorAsync(Guid questionnaireId);
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    private const string ApiPrefix = "api";
    private string Questionnaires => $"{ApiPrefix}/questionnaires";
    private string Questions => $"{ApiPrefix}/questions";
    private string Answers => $"{ApiPrefix}/answers";
    private string Contents => $"{ApiPrefix}/contents";

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // Questionnaire

    public async Task<QuestionnaireDto?> GetQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireDto>();
    }

    public async Task<QuestionnaireInfoDto?> GetLastPublishedQuestionnaireInfoAsync(string questionnaireSlug)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireSlug}/publishes/last/info");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireInfoDto>();
    }

    public async Task<List<QuestionnaireDto>> GetQuestionnairesAsync()
    {
        var response = await _httpClient.GetAsync(Questionnaires);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<QuestionnaireDto>>() ?? new();
    }

    public async Task<QuestionnaireDto?> CreateQuestionnaireAsync(CreateQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Questionnaires, request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<QuestionnaireDto>();
        }

        // Unsuccessful response: Handle the error gracefully.
        var errorBody = await response.Content.ReadAsStringAsync();

        // Deserialize a ProblemDetails response for specific error handling.
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorBody);

        // Throw a custom exception or return an alternative response.
        throw new HttpRequestException(
            $"API request failed with status code {response.StatusCode} and message: {problemDetails?.Detail}");
    }

    public async Task<string?> UpdateQuestionnaireAsync(Guid questionnaireId, UpdateQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Questionnaires}/{questionnaireId}", request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        return null;
    }

    public async Task<string?> UpdateQuestionnaireLookAndFeelAsync(Guid questionnaireId, UpdateLookAndFeelRequestDto request)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{Questionnaires}/{questionnaireId}/look-and-feel", request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        return null;
    }

    public async Task<string?> UpdateContinueButtonAsync(Guid questionnaireId, UpdateContinueButtonRequestDto request)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{Questionnaires}/{questionnaireId}/continue-button", request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        return null;
    }

    public async Task<string?> DeleteQuestionnaireDecorativeImageAsync(Guid questionnaireId)
    {
        var response = await _httpClient.PatchAsync($"{Questionnaires}/{questionnaireId}/decorative-image", null);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        return null;
    }

    public async Task<string?> PublishQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.PatchAsync(
            $"{Questionnaires}/{questionnaireId}?action={QuestionnaireAction.Publish}", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> UnpublishQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.PatchAsync(
            $"{Questionnaires}/{questionnaireId}?action={QuestionnaireAction.Unpublish}", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<QuestionnaireDto?> CloneQuestionnaireAsync(Guid questionnaireId,
        CloneQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync($"{Questionnaires}/{questionnaireId}/clones", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireDto>();
    }

    public async Task<string?> DeleteQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.DeleteAsync($"{Questionnaires}/{questionnaireId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string[]> GetQuestionnaireContributors(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/contributors");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string[]>() ?? [];
    }

    public async Task<string?> RemoveQuestionnaireContributor(Guid questionnaireId, string contributorEmail)
    {
        var response =
            await _httpClient.DeleteAsync($"{Questionnaires}/{questionnaireId}/contributors/{contributorEmail}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> AddQuestionnaireContributor(Guid questionnaireId, AddContributorRequestDto request)
    {
        var response = await _httpClient.PutAsync($"{Questionnaires}/{questionnaireId}/contributors", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    // Question

    public async Task<QuestionDto?> GetQuestionAsync(Guid questionId)
    {
        var response = await _httpClient.GetAsync($"{Questions}/{questionId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionDto>();
    }

    public async Task<List<QuestionDto>> GetQuestionsAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/questions");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<QuestionDto>>() ?? new();
    }

    public async Task<QuestionDto?> CreateQuestionAsync(CreateQuestionRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Questions, request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionDto>();
    }

    public async Task<string?> UpdateQuestionAsync(Guid questionId, UpdateQuestionRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Questions}/{questionId}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string?> UpdateQuestionStatusAsync(Guid questionId, UpdateQuestionStatusRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Questions}/{questionId}/status", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> DeleteQuestionAsync(Guid questionId)
    {
        var response = await _httpClient.DeleteAsync($"{Questions}/{questionId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> MoveQuestionDownOneAsync(Guid questionnaireId, Guid questionId)
    {
        var response = await _httpClient.PatchAsync(
            $"{Questionnaires}/{questionnaireId}/questions/{questionId}?action={QuestionAction.MoveDown}", null);
        response.EnsureSuccessStatusCode();

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        return string.Empty;
    }

    public async Task<string?> MoveQuestionUpOneAsync(Guid questionnaireId, Guid questionId)
    {
        var response = await _httpClient.PatchAsync(
            $"{Questionnaires}/{questionnaireId}/questions/{questionId}?action={QuestionAction.MoveUp}", null);
        response.EnsureSuccessStatusCode();

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        return string.Empty;
    }

    // Answer

    public async Task<AnswerDto?> GetAnswerAsync(Guid answerId)
    {
        var response = await _httpClient.GetAsync($"{Answers}/{answerId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AnswerDto>();
    }

    public async Task<List<AnswerDto>> GetAnswersAsync(Guid questionId)
    {
        var response = await _httpClient.GetAsync($"{Questions}/{questionId}/answers");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<AnswerDto>>() ?? new();
    }

    public async Task<AnswerDto?> CreateAnswerAsync(CreateAnswerRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Answers, request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AnswerDto>();
    }

    public async Task<string?> UpdateAnswerAsync(Guid answerId, UpdateAnswerRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Answers}/{answerId}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> DeleteAnswerAsync(Guid answerId)
    {
        var response = await _httpClient.DeleteAsync($"{Answers}/{answerId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<List<QuestionnaireVersionDto>> GetQuestionnaireVersionsAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/versions");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<QuestionnaireVersionDto>>() ?? new();
    }

    public async Task<QuestionnaireVersionDto?> GetQuestionnaireVersionAsync(Guid questionnaireId, int versionNumber)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/versions/{versionNumber}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireVersionDto?>();
    }

    public async Task<QuestionnaireBranchingMapDto?> GetBranchingMap(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/branching-map");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireBranchingMapDto?>();
    }

    public async Task<ContentDto?> CreateContentAsync(CreateContentRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Contents, request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ContentDto?>();
    }

    public async Task<string?> UpdateContentAsync(Guid id, UpdateContentRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Contents}/{id}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string?>();
    }

    public async Task<string?> DeleteContentAsync(Guid contentId)
    {
        var response = await _httpClient.DeleteAsync($"{Contents}/{contentId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string?>();
    }

    public async Task<ContentDto?> GetContentAsync(Guid contentId)
    {
        var response = await _httpClient.GetAsync($"{Contents}/{contentId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ContentDto?>();
    }

    public async Task<List<ContentDto>> GetContentsAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/contents");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<ContentDto>>() ?? new();
    }

    public async Task<QuestionDto?> GetInitialQuestion(Guid questionnaireId, bool preview = false)
    {
        var response =
            await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/initial-state?preview={preview}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionDto>();
    }

    public async Task<DestinationDto?> GetNextState(Guid questionnaireId, GetNextStateRequest request,
        bool preview = false)
    {
        var response =
            await _httpClient.PostAsJsonAsync($"{Questionnaires}/{questionnaireId}/next-state?preview={preview}",
                request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DestinationDto>();
    }

    public async Task<string?> AddSelfToQuestionnaireContributorAsync(Guid questionnaireId)
    {
        var response = await _httpClient.PutAsync($"{Questionnaires}/{questionnaireId}/contributors/self", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }
}