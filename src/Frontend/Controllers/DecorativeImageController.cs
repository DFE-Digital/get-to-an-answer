using Common.Client;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[Controller]
public class DecorativeImageController(
    ILogger<DecorativeImageController> logger, 
    IApiClient apiClient,
    IImageStorageClient imageStorageClient) : Controller
{
    [HttpGet("/questionnaires/{questionnaireSlug}/decorative-image")]
    public async Task<IActionResult> GetImageFile(string questionnaireSlug)
    {
        try
        {
            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(questionnaireSlug); 
            
            if (questionnaire == null) 
                return NotFound();
            
            // Download the image from blob storage
            var imageStream = await imageStorageClient.DownloadImageAsync(
                $"{questionnaire.Id}/published");

            // Get the file extension
            var extension = Path.GetExtension(questionnaire?.DecorativeImage);
            
            // Return the image stream
            return new FileStreamResult(imageStream, $"image/{extension}");
        }
        catch (FileNotFoundException e)
        {
            logger.LogError(e, "File not found: {FileName}", e.FileName);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while retrieving the image.");
            throw;
        } 
    }
}