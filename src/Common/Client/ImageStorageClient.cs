using Microsoft.Extensions.Logging;

namespace Common.Client;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading.Tasks;

public interface IImageStorageClient
{
    Task UploadImageAsync(Stream fileStream, string blobFileName, string contentType);
    Task<Stream> DownloadImageAsync(string blobFileName);
    Task DeleteImageAsync(string blobFileName);
    Task DuplicateToAsync(string existingBlobFileName, string duplicateBlobFileName);
}

public class ImageStorageClient : IImageStorageClient
{
    private readonly ILogger<ImageStorageClient> _logger;
    
    private readonly string _containerName;
    private readonly BlobContainerClient _containerClient;

    public ImageStorageClient(string connectionString, string containerName, ILogger<ImageStorageClient> logger)
    {
        _containerName = containerName;
        
        logger.LogInformation($"ImageStorageClient initialized with connection string '{connectionString}' and container '{containerName}'.");
        
        _logger = logger;
        
        // 1. Get a reference to the container client
        _containerClient = new BlobContainerClient(connectionString, _containerName);
    }

    /// <summary>
    /// Uploads an image
    /// </summary>
    public async Task UploadImageAsync(Stream fileStream, string blobFileName, string contentType)
    {
        await _containerClient.CreateIfNotExistsAsync(); 

        // 2. Get a reference to the blob client
        BlobClient blobClient = _containerClient.GetBlobClient(blobFileName);

        // 3. Set content type
        var uploadOptions = new BlobUploadOptions
        {
            // Use the content type passed from the web request (e.g., "image/jpeg")
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType } 
        };

        // Use the stream passed into the function directly
        await blobClient.UploadAsync(fileStream, uploadOptions);
        
        _logger.LogInformation($"Image uploaded: {blobFileName}");
    }

    public async Task<Stream> DownloadImageAsync(string blobFileName)
    {
        await CheckContainerExists();
        
        // 2. Get a reference to the blob client
        BlobClient blobClient = _containerClient.GetBlobClient(blobFileName);

        if (!await blobClient.ExistsAsync())
        {
            // Throw an exception or return null if the image doesn't exist
            throw new FileNotFoundException($"Blob '{blobFileName}' not found.");
        }
    
        // 3. Download the blob content into a Stream
        // The DownloadContentAsync method returns a Response<BlobDownloadResult>
        var response = await blobClient.DownloadContentAsync();

        // The Content.ToStream() returns a ReadOnlyStream from the response, 
        // which holds the downloaded image data.
        return response.Value.Content.ToStream();
    }

    public async Task DeleteImageAsync(string blobFileName)
    {
        await CheckContainerExists();
        
        // 2. Get a reference to the blob client
        BlobClient blobClient = _containerClient.GetBlobClient(blobFileName);

        // 3. Delete the blob if it exists
        // DeleteIfExistsAsync returns true if the blob was deleted, false if it didn't exist.
        bool deleted = await blobClient.DeleteIfExistsAsync();

        if (deleted)
        {
            _logger.LogInformation($"Image deleted successfully: {blobFileName}");
        }
        else
        {
            // This is useful for logging, but not strictly an error if we wanted it gone anyway
            _logger.LogInformation($"Image not found (no deletion required): {blobFileName}");
        }
    }

    public async Task DuplicateToAsync(string existingBlobFileName, string duplicateBlobFileName)
    {
        await _containerClient.GetBlobClient(duplicateBlobFileName)
            .StartCopyFromUriAsync(new Uri(_containerClient.Uri, existingBlobFileName));
    }

    private async Task CheckContainerExists()
    {
        var existsResponse = await _containerClient.ExistsAsync();

        if (!existsResponse.Value)
        {
            _logger.LogError($"Container '{_containerName}' not found.");
            throw new FileNotFoundException($"Container '{_containerName}' not found.");
        }
    }
}