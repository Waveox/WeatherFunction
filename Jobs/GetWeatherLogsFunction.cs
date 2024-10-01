using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WeatherFunction.Services;

namespace WeatherFunction;
//GET /api/logs?from=2023-01-01T00:00:00Z&to=2023-01-31T23:59:59Z
public class GetWeatherLogsFunction
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<GetWeatherLogsFunction> _logger;

    public GetWeatherLogsFunction(IWeatherService weatherService, ILogger<GetWeatherLogsFunction> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    [Function("GetWeatherLogs")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "logs")] HttpRequestData req)
    {
        var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

        if (!DateTime.TryParse(queryParams.Get("from"), out var fromTime) ||
            !DateTime.TryParse(queryParams.Get("to"), out var toTime))
        {
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Please provide valid 'from' and 'to' query parameters.");
            return badRequestResponse;
        }

        _logger.LogInformation($"Querying logs from {fromTime} to {toTime}");

        try
        {
            var logs = await _weatherService.GetWeatherLogsAsync(fromTime, toTime);
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(logs);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve logs from Table Storage.");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Failed to retrieve logs from Table Storage.");
            return errorResponse;
        }
    }
}
