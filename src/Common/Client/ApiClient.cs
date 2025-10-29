using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Domain.Request.Add;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
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
    Task<QuestionnaireVersionDto?> GetQuestionnaireVersionAsync(Guid questionnaireId, int versionNumber);
    Task<QuestionnaireVersionDto?> GetLatestQuestionnaireVersion(Guid questionnaireId);
    
    // === For Content ===
    
    Task<ContentDto?> CreateContentAsync(CreateContentRequestDto request);
    Task<string?> UpdateContentAsync(Guid questionnaireId, UpdateContentRequestDto request);
    Task<string?> DeleteContentAsync(Guid contentId);
    Task<ContentDto?> GetContentAsync(Guid contentId);
    Task<List<ContentDto>> GetContentsAsync(Guid questionnaireId);
    
    // === For Service Users ===
    
    Task<QuestionDto?> GetInitialQuestion(Guid questionnaireId);
    Task<DestinationDto?> GetNextState(Guid questionnaireId, GetNextStateRequest request);
    
    // === For Inviting Contributors ===
    
    Task<string?> AddSelfToQuestionnaireContributorAsync(Guid questionnaireId);
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    // Questionnaire
    
    public async Task<QuestionnaireDto?> GetQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireDto>();
    }
    
    public async Task<QuestionnaireInfoDto?> GetLastPublishedQuestionnaireInfoAsync(string questionnaireSlug)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireSlug}/publishes/last/info");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireInfoDto>();
    }

    public async Task<List<QuestionnaireDto>> GetQuestionnairesAsync()
    {
        var response = await _httpClient.GetAsync("questionnaires");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<QuestionnaireDto>>() ?? new ();
    }

    public async Task<QuestionnaireDto?> CreateQuestionnaireAsync(CreateQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("questionnaires", request);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<QuestionnaireDto>();
        }

        // Unsuccessful response: Handle the error gracefully.
        var errorBody = await response.Content.ReadAsStringAsync();
        
        // Deserialize a ProblemDetails response for specific error handling.
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorBody);

        // Throw a custom exception or return an alternative response.
        throw new HttpRequestException($"API request failed with status code {response.StatusCode} and message: {problemDetails?.Detail}");
    }

    public async Task<string?> UpdateQuestionnaireAsync(Guid questionnaireId, UpdateQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"questionnaires/{questionnaireId}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> PublishQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.PutAsync($"questionnaires/{questionnaireId}/publish", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> UnpublishQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.DeleteAsync($"questionnaires/{questionnaireId}/unpublish");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<QuestionnaireDto?> CloneQuestionnaireAsync(Guid questionnaireId, CloneQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync($"questionnaires/{questionnaireId}/clones", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireDto>();
    }

    public async Task<string?> DeleteQuestionnaireAsync(Guid questionnaireId)
    {
        var response = await _httpClient.DeleteAsync($"questionnaires/{questionnaireId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string[]> GetQuestionnaireContributors(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/contributors");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string[]>() ?? [];
    }

    public async Task<string?> RemoveQuestionnaireContributor(Guid questionnaireId, string contributorEmail)
    {
        var response = await _httpClient.DeleteAsync($"questionnaires/{questionnaireId}/contributors/{contributorEmail}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<string>();   
    }
    
    public async Task<string?> AddQuestionnaireContributor(Guid questionnaireId, AddContributorRequestDto request)
    {
        var response = await _httpClient.PutAsync($"questionnaires/{questionnaireId}/contributors", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    // Question

    public async Task<QuestionDto?> GetQuestionAsync(Guid questionId)
    {
        var response = await _httpClient.GetAsync($"questions/{questionId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionDto>();
    }

    public async Task<List<QuestionDto>> GetQuestionsAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/questions"); // "products" is relative to the base URL
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<QuestionDto>>() ?? new ();
    }

    public async Task<QuestionDto?> CreateQuestionAsync(CreateQuestionRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("questions", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionDto>();
    }

    public async Task<string?> UpdateQuestionAsync(Guid questionId, UpdateQuestionRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"questions/{questionId}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }
    
    public async Task<string?> UpdateQuestionStatusAsync(Guid questionId, UpdateQuestionStatusRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"questions/{questionId}/status", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> DeleteQuestionAsync(Guid questionId)
    {
        var response = await _httpClient.DeleteAsync($"questions/{questionId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> MoveQuestionDownOneAsync(Guid questionnaireId, Guid questionId)
    {
        var response = await _httpClient.PutAsync($"questionnaires/{questionnaireId}/questions/{questionId}/move-down", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> MoveQuestionUpOneAsync(Guid questionnaireId, Guid questionId)
    {
        var response = await _httpClient.PutAsync($"questionnaires/{questionnaireId}/questions/{questionId}/move-up", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    // Answer

    public async Task<AnswerDto?> GetAnswerAsync(Guid answerId)
    {
        var response = await _httpClient.GetAsync($"answers/{answerId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AnswerDto>();
    }

    public async Task<List<AnswerDto>> GetAnswersAsync(Guid questionId)
    {
        var response = await _httpClient.GetAsync($"questions/{questionId}/answers");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<AnswerDto>>() ?? new ();
    }

    public async Task<AnswerDto?> CreateAnswerAsync(CreateAnswerRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("answers", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AnswerDto>();
    }

    public async Task<string?> UpdateAnswerAsync(Guid answerId, UpdateAnswerRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"answers/{answerId}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> DeleteAnswerAsync(Guid answerId)
    {
        var response = await _httpClient.DeleteAsync($"answers/{answerId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<List<QuestionnaireVersionDto>> GetQuestionnaireVersionsAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/versions");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<QuestionnaireVersionDto>>() ?? new ();
    }

    public async Task<QuestionnaireVersionDto?> GetQuestionnaireVersionAsync(Guid questionnaireId, int versionNumber)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/versions/{versionNumber}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireVersionDto?>();
    }

    public async Task<QuestionnaireVersionDto?> GetLatestQuestionnaireVersion(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/versions/current");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireVersionDto?>();
    }

    public async Task<ContentDto?> CreateContentAsync(CreateContentRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync($"contents", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ContentDto?>();
    }

    public async Task<string?> UpdateContentAsync(Guid id, UpdateContentRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"contents/{id}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string?>();
    }

    public async Task<string?> DeleteContentAsync(Guid contentId)
    {
        var response = await _httpClient.DeleteAsync($"contents/{contentId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string?>();
    }

    public async Task<ContentDto?> GetContentAsync(Guid contentId)
    {
        var response = await _httpClient.GetAsync($"contents/{contentId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ContentDto?>();
    }

    public async Task<List<ContentDto>> GetContentsAsync(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/contents");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<ContentDto>>() ?? new ();
    }

    public async Task<QuestionDto?> GetInitialQuestion(Guid questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/initial");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionDto>();
    }

    public async Task<DestinationDto?> GetNextState(Guid questionnaireId, GetNextStateRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"questionnaires/{questionnaireId}/next", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DestinationDto>();
    }

    public async Task<string?> AddSelfToQuestionnaireContributorAsync(Guid questionnaireId)
    {
        var response = await _httpClient.PutAsync($"questionnaires/{questionnaireId}/contributors/self", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    } 
}