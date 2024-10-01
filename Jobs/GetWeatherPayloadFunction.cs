using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WeatherFunction.Services;

namespace WeatherFunction;

public class GetWeatherPayloadFunction
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<GetWeatherPayloadFunction> _logger;

    public GetWeatherPayloadFunction(IWeatherService weatherService, ILogger<GetWeatherPayloadFunction> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    [Function("GetWeatherPayloadByLogId")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "payload/{logId}")] HttpRequestData req,
        string logId)
    {
        _logger.LogInformation($"Fetching payload for Log ID: {logId}");

        if (string.IsNullOrWhiteSpace(logId) || !Guid.TryParse(logId, out var id))
        {
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Please provide valid 'logId' query parameter.");
            return badRequestResponse;
        }

        try
        {
            var payload = await _weatherService.GetWeatherPayloadAsync(logId);

            if (string.IsNullOrWhiteSpace(payload))
            {
                var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Payload for blob not found.");
                return notFoundResponse;
            }

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync(payload);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve payload from Blob Storage.");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Failed to retrieve payload.");
            return errorResponse;
        }
    }
}
