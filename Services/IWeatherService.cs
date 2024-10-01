using WeatherFunction.Models;

namespace WeatherFunction.Services;
public interface IWeatherService
{
    Task FetchWeatherDataAsync();
    Task<List<WeatherEntity>?> GetWeatherLogsAsync(DateTime fromTime, DateTime toTime);
    Task<string?> GetWeatherPayloadAsync(string logId);
}