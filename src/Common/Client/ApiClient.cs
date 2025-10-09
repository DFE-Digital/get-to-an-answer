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
    Task<QuestionnaireDto?> GetQuestionnaireAsync(int questionnaireId);
    Task<List<QuestionnaireDto>> GetQuestionnairesAsync();
    Task<QuestionnaireDto?> CreateQuestionnaireAsync(CreateQuestionnaireRequestDto request);
    Task<string?> UpdateQuestionnaireAsync(int questionnaireId, UpdateQuestionnaireRequestDto request);
    Task<string?> DeleteQuestionnaireAsync(int questionnaireId);
    
    Task<QuestionDto?> GetQuestionAsync(int questionId);
    Task<List<QuestionDto>> GetQuestionsAsync(int questionnaireId);
    Task<QuestionDto?> CreateQuestionAsync(CreateQuestionRequestDto request);
    Task<string?> UpdateQuestionAsync(int questionId, UpdateQuestionRequestDto request);
    Task<string?> UpdateQuestionStatusAsync(int questionId, UpdateQuestionStatusRequestDto request);
    Task<string?> DeleteQuestionAsync(int questionId);
    
    Task<AnswerDto?> GetAnswerAsync(int answerId);
    Task<List<AnswerDto>> GetAnswersAsync(int questionId);
    Task<AnswerDto?> CreateAnswerAsync(CreateAnswerRequestDto request);
    Task<string?> UpdateAnswerAsync(int answerId, UpdateAnswerRequestDto request);
    Task<string?> DeleteAnswerAsync(int answerId);
    
    // === For Service Users ===
    
    Task<QuestionDto?> GetInitialQuestion(int questionnaireId);
    Task<DestinationDto?> GetNextState(int questionnaireId, GetNextStateRequest request);
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
        else
        {
            // Unsuccessful response: Handle the error gracefully.
            var errorBody = await response.Content.ReadAsStringAsync();
        
            // Deserialize a ProblemDetails response for specific error handling.
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorBody);

            // Throw a custom exception or return an alternative response.
            throw new HttpRequestException($"API request failed with status code {response.StatusCode} and message: {problemDetails?.Detail}");
        }
    }

    public async Task<string?> UpdateQuestionnaireAsync(int questionnaireId, UpdateQuestionnaireRequestDto request)
    {
        var response = await _httpClient.PutAsJsonAsync($"questionnaires/{questionnaireId}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<string>();
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
}