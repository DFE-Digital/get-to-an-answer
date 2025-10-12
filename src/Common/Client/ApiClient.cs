using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Client;

public interface IApiClient
{
    // === For Questionnaires ===
    
    Task<QuestionnaireDto?> GetQuestionnaireAsync(int questionnaireId);
    Task<List<QuestionnaireDto>> GetQuestionnairesAsync();
    Task<QuestionnaireDto?> CreateQuestionnaireAsync(CreateQuestionnaireRequestDto request);
    Task<string?> UpdateQuestionnaireAsync(int questionnaireId, UpdateQuestionnaireRequestDto request);
    Task<string?> UpdateQuestionnaireStatusAsync(int questionnaireId, UpdateQuestionnaireStatusRequestDto request);
    Task<QuestionnaireDto?> CloneQuestionnaireAsync(int questionnaireId, CloneQuestionnaireRequestDto request);
    Task<string?> DeleteQuestionnaireAsync(int questionnaireId);
    
    // === For Questions ===
    
    Task<QuestionDto?> GetQuestionAsync(int questionId);
    Task<List<QuestionDto>> GetQuestionsAsync(int questionnaireId);
    Task<QuestionDto?> CreateQuestionAsync(CreateQuestionRequestDto request);
    Task<string?> UpdateQuestionAsync(int questionId, UpdateQuestionRequestDto request);
    Task<string?> UpdateQuestionStatusAsync(int questionId, UpdateQuestionStatusRequestDto request);
    Task<string?> DeleteQuestionAsync(int questionId);
    
    // === For Answers ===
    
    Task<AnswerDto?> GetAnswerAsync(int answerId);
    Task<List<AnswerDto>> GetAnswersAsync(int questionId);
    Task<AnswerDto?> CreateAnswerAsync(CreateAnswerRequestDto request);
    Task<string?> UpdateAnswerAsync(int answerId, UpdateAnswerRequestDto request);
    Task<string?> DeleteAnswerAsync(int answerId);
    
    // === For Questionnaire Versions ===
    
    Task<List<QuestionnaireVersionDto>> GetQuestionnaireVersionsAsync(int questionnaireId);
    Task<QuestionnaireVersionDto?> GetQuestionnaireVersionAsync(int questionnaireId, int versionNumber);
    Task<QuestionnaireVersionDto?> GetLatestQuestionnaireVersion(int questionnaireId);
    
    // === For Service Users ===
    
    Task<QuestionDto?> GetInitialQuestion(int questionnaireId);
    Task<DestinationDto?> GetNextState(int questionnaireId, GetNextStateRequest request);
    
    // === For Inviting Contributors ===
    
    Task<string?> AddSelfToQuestionnaireContributorAsync(int questionnaireId);
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
    
    public async Task<QuestionnaireDto?> GetQuestionnaireAsync(int questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireDto>();
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

    public async Task<string?> UpdateQuestionnaireAsync(int questionnaireId, UpdateQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"questionnaires/{questionnaireId}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> UpdateQuestionnaireStatusAsync(int questionnaireId, UpdateQuestionnaireStatusRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"questionnaires/{questionnaireId}/status", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<QuestionnaireDto?> CloneQuestionnaireAsync(int questionnaireId, CloneQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync($"questionnaires/{questionnaireId}/clones", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireDto>();
    }

    public async Task<string?> DeleteQuestionnaireAsync(int questionnaireId)
    {
        var response = await _httpClient.DeleteAsync($"questionnaires/{questionnaireId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }
    
    // Question

    public async Task<QuestionDto?> GetQuestionAsync(int questionId)
    {
        var response = await _httpClient.GetAsync($"questions/{questionId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionDto>();
    }

    public async Task<List<QuestionDto>> GetQuestionsAsync(int questionnaireId)
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

    public async Task<string?> UpdateQuestionAsync(int questionId, UpdateQuestionRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"questions/{questionId}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }
    
    public async Task<string?> UpdateQuestionStatusAsync(int questionId, UpdateQuestionStatusRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"questions/{questionId}/status", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> DeleteQuestionAsync(int questionId)
    {
        var response = await _httpClient.DeleteAsync($"questions/{questionId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }
    
    // Answer

    public async Task<AnswerDto?> GetAnswerAsync(int answerId)
    {
        var response = await _httpClient.GetAsync($"answers/{answerId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AnswerDto>();
    }

    public async Task<List<AnswerDto>> GetAnswersAsync(int questionId)
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

    public async Task<string?> UpdateAnswerAsync(int answerId, UpdateAnswerRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"answers/{answerId}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string?> DeleteAnswerAsync(int answerId)
    {
        var response = await _httpClient.DeleteAsync($"answers/{answerId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<List<QuestionnaireVersionDto>> GetQuestionnaireVersionsAsync(int questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/versions");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<QuestionnaireVersionDto>>() ?? new ();
    }

    public async Task<QuestionnaireVersionDto?> GetQuestionnaireVersionAsync(int questionnaireId, int versionNumber)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/versions/{versionNumber}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireVersionDto?>();
    }

    public async Task<QuestionnaireVersionDto?> GetLatestQuestionnaireVersion(int questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/versions/current");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionnaireVersionDto?>();
    }

    public async Task<QuestionDto?> GetInitialQuestion(int questionnaireId)
    {
        var response = await _httpClient.GetAsync($"questionnaires/{questionnaireId}/initial");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<QuestionDto>();
    }

    public async Task<DestinationDto?> GetNextState(int questionnaireId, GetNextStateRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"questionnaires/{questionnaireId}/next", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DestinationDto>();
    }

    public async Task<string?> AddSelfToQuestionnaireContributorAsync(int questionnaireId)
    {
        var response = await _httpClient.PutAsync($"questionnaires/{questionnaireId}/contributors", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
    } 
}