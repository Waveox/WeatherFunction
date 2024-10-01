using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using WeatherFunction.Configurations;
using WeatherFunction.Models;

namespace WeatherFunction.Services.Implementations;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly WeatherFunctionOptions _options;

    public BlobStorageService(IOptions<WeatherFunctionOptions> options)
    {
        _options = options.Value;
        // On production ConnectionString should be stored in Enviroment Variables/Azure KeyVault
        var blobServiceClient = new BlobServiceClient(_options.AzureBlobStorage.ConnectionString);
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(_options.AzureBlobStorage.ContainerName);

        _blobContainerClient.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task StoreBlobAsync(string blobName, WeatherData data)
    {
        var blobClient = _blobContainerClient.GetBlobClient(blobName);

        string jsonData = JsonSerializer.Serialize(data);
        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonData)))
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }
    }

    public async Task<string> GetBlobAsync(string blobName)
    {
        var blobClient = _blobContainerClient.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync())
        {
            var blobContent = await blobClient.DownloadContentAsync();
            return blobContent.Value.Content.ToString();
        }

        return string.Empty;
    }
}
