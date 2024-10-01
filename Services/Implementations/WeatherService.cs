using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherFunction.Configurations;
using WeatherFunction.Models;

namespace WeatherFunction.Services.Implementations;

public class WeatherService : IWeatherService
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ITableStorageService _tableStorageService;
    private readonly ILogger<WeatherService> _logger;
    private readonly WeatherFunctionOptions _options;
    private static readonly HttpClient _httpClient = new HttpClient();
    public WeatherService(IBlobStorageService blobStorageService, ITableStorageService tableStorageService, IOptions<WeatherFunctionOptions> options, ILogger<WeatherService> logger)
    {
        _blobStorageService = blobStorageService;
        _tableStorageService = tableStorageService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task FetchWeatherDataAsync()
    {
        // On production Secret should be stored in Enviroment Variables/Azure KeyVault
        string apiUrl = $"{_options.OpenWeather.Endpoint}{_options.OpenWeather.ApiKey}";
        var blobName = $"{Guid.NewGuid()}_weatherdata";

        try
        {
            var weatherData = await _httpClient.GetFromJsonAsync<WeatherData>(apiUrl);

            if (weatherData == null)
            {
                await _tableStorageService.LogWeatherCallAsync(false, string.Empty, DateTime.UtcNow);
                _logger.LogError($"Failed to parse weather data");
                return;
            }

            await _tableStorageService.LogWeatherCallAsync(true, blobName, DateTime.UtcNow);
            await _blobStorageService.StoreBlobAsync(blobName, weatherData);

            _logger.LogInformation($"Weather data fetched and stored successfully at: {DateTime.UtcNow}");
        }
        catch (Exception ex)
        {
            await _tableStorageService.LogWeatherCallAsync(false, string.Empty, DateTime.UtcNow, ex.Message);

            _logger.LogError($"Failed to fetch weather data: {ex.Message}");
        }
    }

    public async Task<string?> GetWeatherPayloadAsync(string id)
    {
        try
        {
            var log = await _tableStorageService.GetWeatherLogByIdAsync(id);

            if (log == null)
            {
                _logger.LogError($"Failed to retrieve weather entity from Table Storage with id: {id}");
                return null;
            }

            var payload = await _blobStorageService.GetBlobAsync(log.BlobName);

            return payload ?? null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve weather payload from Blob Storage with id: {id}.");
            return null;
        }
    }

    public async Task<List<WeatherEntity>?> GetWeatherLogsAsync(DateTime fromTime, DateTime toTime) => await _tableStorageService.GetLogsAsync(fromTime, toTime);
}
