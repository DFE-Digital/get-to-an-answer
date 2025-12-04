using System.Net;
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
using Microsoft.Extensions.Logging;

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
    Task<string?> RemoveQuestionnaireContributor(Guid questionnaireId, string contributorId);
    Task<string?> UpdateCompletionStateAsync(Guid questionnaireId, UpdateCompletionStateRequestDto request);

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
    Task<string?> BulkUpsertAnswersAsync(BulkUpsertAnswersRequestDto bulkRequest);
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
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    
    private readonly ILogger<ApiClient> _logger;

    private const string ApiPrefix = "api";
    private string Questionnaires => $"{ApiPrefix}/questionnaires";
    private string Questions => $"{ApiPrefix}/questions";
    private string Answers => $"{ApiPrefix}/answers";
    private string Contents => $"{ApiPrefix}/contents";

    public ApiClient(ILogger<ApiClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // Questionnaire

    public async Task<QuestionnaireDto?> GetQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}");
        return await GetResponse<QuestionnaireDto>(response);
    }

    public async Task<QuestionnaireInfoDto?> GetLastPublishedQuestionnaireInfoAsync(string questionnaireSlug)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireSlug}/publishes/last/info");
        return await GetResponse<QuestionnaireInfoDto>(response);
    }

    public async Task<List<QuestionnaireDto>> GetQuestionnairesAsync()
    {
        var response = await _httpClient.GetAsync(Questionnaires);
        return await GetResponse<List<QuestionnaireDto>>(response) ?? new();
    }

    public async Task<QuestionnaireDto?> CreateQuestionnaireAsync(CreateQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Questionnaires, request);
        return await GetResponse<QuestionnaireDto>(response);
    }

    public async Task<string?> UpdateQuestionnaireAsync(Guid questionnaireId, UpdateQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Questionnaires}/{questionnaireId}", request);
        return await GetResponse<string>(response);
    }

    public async Task<string?> UpdateQuestionnaireLookAndFeelAsync(Guid questionnaireId, UpdateLookAndFeelRequestDto request)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{Questionnaires}/{questionnaireId}/look-and-feel", request);
        return await GetResponse<string>(response);
    }

    public async Task<string?> UpdateContinueButtonAsync(Guid questionnaireId, UpdateContinueButtonRequestDto request)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{Questionnaires}/{questionnaireId}/continue-button", request);
        return await GetResponse<string>(response);
    }

    public async Task<string?> DeleteQuestionnaireDecorativeImageAsync(Guid questionnaireId)
    {
        var response = await _httpClient.PatchAsync($"{Questionnaires}/{questionnaireId}/decorative-image", null);
        return await GetResponse<string>(response);
    }

    public async Task<string?> PublishQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.PatchAsync(
            $"{Questionnaires}/{questionnaireId}?action={QuestionnaireAction.Publish}", null);
        return await GetResponse<string>(response);
    }

    public async Task<string?> UnpublishQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.PatchAsync(
            $"{Questionnaires}/{questionnaireId}?action={QuestionnaireAction.Unpublish}", null);
        return await GetResponse<string>(response);
    }

    public async Task<QuestionnaireDto?> CloneQuestionnaireAsync(Guid questionnaireId,
        CloneQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync($"{Questionnaires}/{questionnaireId}/clones", request);
        return await GetResponse<QuestionnaireDto>(response);
    }

    public async Task<string?> DeleteQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.DeleteAsync($"{Questionnaires}/{questionnaireId}");
        return await GetResponse<string>(response);
    }

    public async Task<string[]> GetQuestionnaireContributors(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/contributors");
        return await GetResponse<string[]>(response) ?? [];
    }

    public async Task<string?> RemoveQuestionnaireContributor(Guid questionnaireId, string contributorId)
    {
        var response =
            await _httpClient.DeleteAsync($"{Questionnaires}/{questionnaireId}/contributors/{contributorId}");
        return await GetResponse<string>(response);
    }

    public async Task<string?> UpdateCompletionStateAsync(Guid questionnaireId, UpdateCompletionStateRequestDto request)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{Questionnaires}/{questionnaireId}/completion-state", request);
        return await GetResponse<string>(response);
    }

    public async Task<string?> AddQuestionnaireContributor(Guid questionnaireId, AddContributorRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Questionnaires}/{questionnaireId}/contributors", request);
        return await GetResponse<string>(response);
    }

    // Question

    public async Task<QuestionDto?> GetQuestionAsync(Guid questionId)
    {
        var response = await _httpClient.GetAsync($"{Questions}/{questionId}");
        return await GetResponse<QuestionDto>(response);
    }

    public async Task<List<QuestionDto>> GetQuestionsAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/questions");
        return await GetResponse<List<QuestionDto>>(response) ?? new();
    }

    public async Task<QuestionDto?> CreateQuestionAsync(CreateQuestionRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Questions, request);
        return await GetResponse<QuestionDto>(response);
    }

    public async Task<string?> UpdateQuestionAsync(Guid questionId, UpdateQuestionRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Questions}/{questionId}", request);
        return await GetResponse<string>(response);
    }

    public async Task<string?> UpdateQuestionStatusAsync(Guid questionId, UpdateQuestionStatusRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Questions}/{questionId}/status", request);
        return await GetResponse<string>(response);
    }

    public async Task<string?> DeleteQuestionAsync(Guid questionId)
    {
        var response = await _httpClient.DeleteAsync($"{Questions}/{questionId}");
        return await GetResponse<string>(response);
    }

    public async Task<string?> MoveQuestionDownOneAsync(Guid questionnaireId, Guid questionId)
    {
        var response = await _httpClient.PatchAsync(
            $"{Questionnaires}/{questionnaireId}/questions/{questionId}?action={QuestionAction.MoveDown}", null);
        return await GetResponse<string>(response);
    }

    public async Task<string?> MoveQuestionUpOneAsync(Guid questionnaireId, Guid questionId)
    {
        var response = await _httpClient.PatchAsync(
            $"{Questionnaires}/{questionnaireId}/questions/{questionId}?action={QuestionAction.MoveUp}", null);
        return await GetResponse<string>(response);
    }

    // Answer

    public async Task<AnswerDto?> GetAnswerAsync(Guid answerId)
    {
        var response = await _httpClient.GetAsync($"{Answers}/{answerId}");
        return await GetResponse<AnswerDto>(response);
    }

    public async Task<List<AnswerDto>> GetAnswersAsync(Guid questionId)
    {
        var response = await _httpClient.GetAsync($"{Questions}/{questionId}/answers");
        return await GetResponse<List<AnswerDto>>(response) ?? new();
    }

    public async Task<AnswerDto?> CreateAnswerAsync(CreateAnswerRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Answers, request);
        return await GetResponse<AnswerDto>(response);
    }

    public async Task<string?> UpdateAnswerAsync(Guid answerId, UpdateAnswerRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Answers}/{answerId}", request);
        return await GetResponse<string>(response);
    }

    public async Task<string?> BulkUpsertAnswersAsync(BulkUpsertAnswersRequestDto bulkRequest)
    {
        var response = await _httpClient.PostAsJsonAsync($"{Answers}/bulk", bulkRequest);
        return await GetResponse<string>(response);
    }

    public async Task<string?> DeleteAnswerAsync(Guid answerId)
    {
        var response = await _httpClient.DeleteAsync($"{Answers}/{answerId}");
        return await GetResponse<string>(response);
    }

    public async Task<List<QuestionnaireVersionDto>> GetQuestionnaireVersionsAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/versions");
        return await GetResponse<List<QuestionnaireVersionDto>>(response) ?? new();
    }

    public async Task<QuestionnaireBranchingMapDto?> GetBranchingMap(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/branching-map");
        return await GetResponse<QuestionnaireBranchingMapDto>(response);
    }

    public async Task<ContentDto?> CreateContentAsync(CreateContentRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Contents, request);
        return await GetResponse<ContentDto>(response);
    }

    public async Task<string?> UpdateContentAsync(Guid id, UpdateContentRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{Contents}/{id}", request);
        return await GetResponse<string>(response);
    }

    public async Task<string?> DeleteContentAsync(Guid contentId)
    {
        var response = await _httpClient.DeleteAsync($"{Contents}/{contentId}");
        return await GetResponse<string>(response);
    }

    public async Task<ContentDto?> GetContentAsync(Guid contentId)
    {
        var response = await _httpClient.GetAsync($"{Contents}/{contentId}");
        return await GetResponse<ContentDto>(response);
    }

    public async Task<List<ContentDto>> GetContentsAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/contents");
        return await GetResponse<List<ContentDto>>(response) ?? new();
    }

    public async Task<QuestionDto?> GetInitialQuestion(Guid questionnaireId, bool preview = false)
    {
        var response =
            await _httpClient.GetAsync($"{Questionnaires}/{questionnaireId}/initial-state?preview={preview}");
        return await GetResponse<QuestionDto>(response);
    }

    public async Task<DestinationDto?> GetNextState(Guid questionnaireId, GetNextStateRequest request,
        bool preview = false)
    {
        var response =
            await _httpClient.PostAsJsonAsync($"{Questionnaires}/{questionnaireId}/next-state?preview={preview}",
                request);
        return await GetResponse<DestinationDto>(response);
    }

    private async Task<TResponse?> GetResponse<TResponse>(HttpResponseMessage response) where TResponse : class
    {
        if (response.IsSuccessStatusCode)
        {
            if (typeof(TResponse) == typeof(string))
            {
                return await response.Content.ReadAsStringAsync() as TResponse;
            }
            
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        // Unsuccessful response: Handle the error gracefully.
        var errorBody = await response.Content.ReadAsStringAsync();

        try
        {
            // Deserialize a ProblemDetails response for specific error handling.
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorBody);
            
            var statusCode = problemDetails?.Status ?? (int) response.StatusCode;

            _logger.LogError(problemDetails?.Detail);

            // Throw a custom exception or return an alternative response.
            throw new GetToAnAnswerApiException(
                $"API request failed with status code {response.StatusCode} and message: {problemDetails?.Detail}", 
                problemDetails, (HttpStatusCode) statusCode);
        }
        catch (Exception)
        {
            _logger.LogError($"API request '{response.RequestMessage?.RequestUri?.AbsolutePath}' failed with status code '{response.StatusCode}', error: '{errorBody}'");
            throw;
        }
    }
}

public class GetToAnAnswerApiException : Exception
{
    public GetToAnAnswerApiException(string? message, ProblemDetails? problemDetails, HttpStatusCode? statusCode)
        : base(message, null)
    {
        StatusCode = statusCode;
        ProblemDetails = problemDetails;
    }
    
    /// <value>
    /// An HTTP status code if the exception represents a non-successful result, otherwise <c>null</c>.
    /// </value>
    public HttpStatusCode? StatusCode { get; }
    
    public ProblemDetails? ProblemDetails { get; }
}