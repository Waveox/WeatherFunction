namespace WeatherFunction.Configurations;
public class WeatherFunctionOptions
{
    public OpenWeatherOptions OpenWeather { get; set; }
    public AzureBlobStorageOptions AzureBlobStorage { get; set; }
    public AzureTableStorageOptions AzureTableStorage { get; set; }
    public WeatherStorageOptions WeatherStorage { get; set; }

    public class OpenWeatherOptions
    {
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
    }

    public class AzureBlobStorageOptions
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }

    public class AzureTableStorageOptions
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
    }

    public class WeatherStorageOptions
    {
        public string PartitionKey { get; set; }
    }
}