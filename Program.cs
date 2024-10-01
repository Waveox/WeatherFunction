using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WeatherFunction.Services;
using WeatherFunction.Services.Implementations;
using WeatherFunction.Models;
using WeatherFunction.Configurations;
using Microsoft.Extensions.Options;
using Serilog;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, builder) =>
    {
        string contentRootPath = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot") ?? context.HostingEnvironment.ContentRootPath;

        builder
            .SetBasePath(contentRootPath)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.Configure<WeatherFunctionOptions>(context.Configuration);
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<ITableStorageService, TableStorageService>();
        services.AddScoped<IWeatherService, WeatherService>();
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<WeatherFunctionOptions>>();
        WeatherEntity.Initialize(options);
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
