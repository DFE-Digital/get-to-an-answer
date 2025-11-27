using Common.Client;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[Controller]
public class DecorativeImageController(
    ILogger<DecorativeImageController> logger, 
    IApiClient apiClient,
    IImageStorageClient imageStorageClient) : Controller
{
    [HttpGet("/questionnaires/{questionnaireId:guid}/decorative-image")]
    public async Task<IActionResult> GetImageFile(Guid questionnaireId)
    {
        try
        {
            var questionnaire = await apiClient.GetQuestionnaireAsync(questionnaireId); 
            
            // Download the image from blob storage
            var imageStream = await imageStorageClient.DownloadImageAsync(
                $"{questionnaireId}/{questionnaire?.DecorativeImage}");

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