using WeatherFunction.Models;

namespace WeatherFunction.Services;
public interface ITableStorageService
{
    Task LogWeatherCallAsync(bool success, string blobName, DateTime timestampUtc, string errorMessage = null);
    Task<WeatherEntity> GetWeatherLogByIdAsync(string id);
    Task<List<WeatherEntity>> GetLogsAsync(DateTime fromTime, DateTime toTime);
}