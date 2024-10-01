using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WeatherFunction.Services;

namespace WeatherFunction;

public class FetchWeatherFunction
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<FetchWeatherFunction> _logger;

    public FetchWeatherFunction(IWeatherService weatherService, ILogger<FetchWeatherFunction> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    [Function("FetchWeatherTimerTrigger")]
    public async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo timer)
    {
        _logger.LogInformation($"Fetch weather data function executed at: {DateTime.UtcNow}");

        try
        {
            await _weatherService.FetchWeatherDataAsync();
            _logger.LogInformation("Weather data fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch weather data.");
        }
    }
}