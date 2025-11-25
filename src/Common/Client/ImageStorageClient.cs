namespace Common.Client;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading.Tasks;

public class ImageStorageClient
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public ImageStorageClient(string connectionString, string containerName)
    {
        _connectionString = connectionString;
        _containerName = containerName;
    }

    /// <summary>
    /// Uploads an image from a local file path to the blob storage.
    /// </summary>
    /// <param name="localFilePath">Path to the image file on disk.</param>
    /// <param name="blobFileName">The desired name for the file in blob storage.</param>
    public async Task UploadImageAsync(string localFilePath, string blobFileName)
    {
        // 1. Get a reference to the container client
        BlobContainerClient containerClient = new BlobContainerClient(_connectionString, _containerName);
        
        // Ensure the container exists (optional, as Terraform created it)
        await containerClient.CreateIfNotExistsAsync(); 

        // 2. Get a reference to the blob client
        BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

        // 3. Set content type and upload the file
        // We assume it's a JPEG or PNG and set the content type
        string contentType = Path.GetExtension(localFilePath).ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        using (FileStream uploadFileStream = File.OpenRead(localFilePath))
        {
            await blobClient.UploadAsync(uploadFileStream, uploadOptions);
        }

        System.Console.WriteLine($"Image uploaded: {blobFileName}");
    }

    /// <summary>
    /// Retrieves an image from blob storage and saves it to a local file path.
    /// </summary>
    /// <param name="blobFileName">The name of the file in blob storage.</param>
    /// <param name="downloadFilePath">The path where the file should be saved locally.</param>
    public async Task DownloadImageAsync(string blobFileName, string downloadFilePath)
    {
        // 1. Get a reference to the container client
        BlobContainerClient containerClient = new BlobContainerClient(_connectionString, _containerName);

        // 2. Get a reference to the blob client
        BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

        if (!await blobClient.ExistsAsync())
        {
            System.Console.WriteLine($"Error: Blob '{blobFileName}' does not exist.");
            return;
        }

        // 3. Download the blob content to a local file path
        await blobClient.DownloadToAsync(downloadFilePath);

        System.Console.WriteLine($"Image downloaded to: {downloadFilePath}");
    }
}