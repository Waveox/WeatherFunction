using WeatherFunction.Models;

namespace WeatherFunction.Services;
public interface IBlobStorageService
{
    Task StoreBlobAsync(string blobName, WeatherData data);
    Task<string> GetBlobAsync(string blobName);
}