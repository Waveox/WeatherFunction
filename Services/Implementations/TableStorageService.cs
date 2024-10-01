using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using WeatherFunction.Configurations;
using WeatherFunction.Models;

namespace WeatherFunction.Services.Implementations;

public class TableStorageService : ITableStorageService
{
    private readonly TableClient _tableClient;
    private readonly WeatherFunctionOptions _options;

    public TableStorageService(IOptions<WeatherFunctionOptions> options)
    {
        _options = options.Value;
        // On production ConnectionString should be stored in Enviroment Variables/Azure KeyVault
        var serviceClient = new TableServiceClient(_options.AzureTableStorage.ConnectionString);
        _tableClient = serviceClient.GetTableClient(_options.AzureTableStorage.TableName);

        _tableClient.CreateIfNotExists();
    }

    public async Task LogWeatherCallAsync(bool success, string blobName, DateTime timestamp, string errorMessage = null)
    {
        var logEntity = new WeatherEntity()
        {
            Success = success,
            BlobName = blobName,
            TimestampUtc = timestamp,
            ErrorMessage = errorMessage
        };

        await _tableClient.AddEntityAsync(logEntity);
    }

    public async Task<WeatherEntity> GetWeatherLogByIdAsync(string id) => await _tableClient.GetEntityAsync<WeatherEntity>(_options.WeatherStorage.PartitionKey, id);

    public async Task<List<WeatherEntity>> GetLogsAsync(DateTime fromTime, DateTime toTime)
    {
        var logs = new List<WeatherEntity>();

        await foreach (var log in _tableClient.QueryAsync<WeatherEntity>(e => e.TimestampUtc >= fromTime && e.TimestampUtc <= toTime))
        {
            logs.Add(log);
        }

        return logs.OrderBy(e => e.TimestampUtc).ToList();
    }
}