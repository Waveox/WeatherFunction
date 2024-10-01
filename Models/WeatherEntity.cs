using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using WeatherFunction.Configurations;

namespace WeatherFunction.Models;
public class WeatherEntity : ITableEntity
{
    private static string _partitionKey;

    public static void Initialize(IOptions<WeatherFunctionOptions> options)
    {
        _partitionKey = options.Value.WeatherStorage.PartitionKey;
    }

    public string PartitionKey { get; set; } = _partitionKey;
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public bool Success { get; set; }
    public string BlobName { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string ErrorMessage { get; set; }
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}