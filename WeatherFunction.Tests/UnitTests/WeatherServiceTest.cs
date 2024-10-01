using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq;
using WeatherFunction.Configurations;
using WeatherFunction.Models;
using WeatherFunction.Services;
using WeatherFunction.Services.Implementations;
using static WeatherFunction.Configurations.WeatherFunctionOptions;

namespace WeatherFunction.Tests.UnitTests;

[TestFixture]
public class WeatherServiceTests
{
    private WeatherService _weatherService;
    private Mock<IBlobStorageService> _blobStorageServiceMock;
    private Mock<ITableStorageService> _tableStorageServiceMock;
    private Mock<ILogger<WeatherService>> _loggerMock;
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private IFixture _fixture;
    private WeatherFunctionOptions _options;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _blobStorageServiceMock = new Mock<IBlobStorageService>();
        _tableStorageServiceMock = new Mock<ITableStorageService>();
        _loggerMock = new Mock<ILogger<WeatherService>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        _options = new WeatherFunctionOptions
        {
            OpenWeather = new OpenWeatherOptions
            {
                Endpoint = "http://fakeapi.com/weather?apiKey=",
                ApiKey = "fakeApiKey"
            }
        };

        _weatherService = new WeatherService(

            _blobStorageServiceMock.Object,
            _tableStorageServiceMock.Object,
            Options.Create(_options),
            _httpClientFactoryMock.Object,
            _loggerMock.Object
        );
    }

    [Test]
    public async Task FetchWeatherDataAsync_ShouldStoreData_WhenApiReturnsWeatherData()
    {
        // Arrange
        var weatherData = _fixture.Create<WeatherData>();
        var apiUrl = $"{_options.OpenWeather.Endpoint}{_options.OpenWeather.ApiKey}";

        var httpClientMock = new HttpClient(new FakeHttpMessageHandler
        {
            ResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(weatherData)
            }
        });

        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClientMock);

        // Act
        await _weatherService.FetchWeatherDataAsync();

        // Assert
        _tableStorageServiceMock.Verify(ts => ts.LogWeatherCallAsync(true, It.IsAny<string>(), It.IsAny<DateTime>(), null), Times.Once);
        _blobStorageServiceMock.Verify(bs => bs.StoreBlobAsync(It.IsAny<string>(), It.IsAny<WeatherData>()), Times.Once);
    }

    [Test]
    public async Task FetchWeatherDataAsync_ShouldNotStoreBlob_WhenApiReturnsNullWeatherData()
    {
        // Arrange
        var apiUrl = $"{_options.OpenWeather.Endpoint}{_options.OpenWeather.ApiKey}";

        var httpClientMock = new HttpClient(new FakeHttpMessageHandler
        {
            ResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null")
            }
        });

        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClientMock);

        // Act
        await _weatherService.FetchWeatherDataAsync();

        // Assert
        _tableStorageServiceMock.Verify(ts => ts.LogWeatherCallAsync(false, string.Empty, It.IsAny<DateTime>(), null), Times.Once);
        _blobStorageServiceMock.Verify(bs => bs.StoreBlobAsync(It.IsAny<string>(), It.IsAny<WeatherData>()), Times.Never);
    }

    [Test]
    public async Task FetchWeatherDataAsync_ShouldNotStoreBlob_WhenExceptionIsThrown()
    {
        // Arrange
        var apiUrl = $"{_options.OpenWeather.Endpoint}{_options.OpenWeather.ApiKey}";

        var httpClientMock = new HttpClient(new FakeHttpMessageHandler
        {
            ShouldThrow = true
        });

        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClientMock);

        // Act
        await _weatherService.FetchWeatherDataAsync();

        // Assert
        _tableStorageServiceMock.Verify(ts => ts.LogWeatherCallAsync(false, string.Empty, It.IsAny<DateTime>(), "Network error"), Times.Once);
        _blobStorageServiceMock.Verify(bs => bs.StoreBlobAsync(It.IsAny<string>(), It.IsAny<WeatherData>()), Times.Never);
    }

    [Test]
    public async Task GetWeatherPayloadAsync_ShouldReturnPayload_WhenLogExists()
    {
        // Arrange
        var logId = Guid.NewGuid().ToString();
        var blobName = "blobName";
        var expectedPayload = "weatherDataPayload";

        var log = new WeatherEntity { BlobName = blobName };
        _tableStorageServiceMock.Setup(ts => ts.GetWeatherLogByIdAsync(logId)).ReturnsAsync(log);
        _blobStorageServiceMock.Setup(bs => bs.GetBlobAsync(blobName)).ReturnsAsync(expectedPayload);

        // Act
        var result = await _weatherService.GetWeatherPayloadAsync(logId);

        // Assert
        Assert.That(expectedPayload, Is.EqualTo(result));
    }

    [Test]
    public async Task GetWeatherPayloadAsync_ShouldReturnNull_WhenLogDoesNotExist()
    {
        // Arrange
        var logId = Guid.NewGuid().ToString();
        _tableStorageServiceMock.Setup(ts => ts.GetWeatherLogByIdAsync(logId)).ReturnsAsync((WeatherEntity)null);

        // Act
        var result = await _weatherService.GetWeatherPayloadAsync(logId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetWeatherPayloadAsync_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Arrange
        var logId = Guid.NewGuid().ToString();
        _tableStorageServiceMock.Setup(ts => ts.GetWeatherLogByIdAsync(logId)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _weatherService.GetWeatherPayloadAsync(logId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetWeatherLogsAsync_ShouldReturnLogs_WhenLogsExistInRange()
    {
        // Arrange
        var fromTime = DateTime.UtcNow.AddDays(-1);
        var toTime = DateTime.UtcNow;
        var logs = _fixture.CreateMany<WeatherEntity>(3).ToList();

        _tableStorageServiceMock
            .Setup(ts => ts.GetLogsAsync(fromTime, toTime))
            .ReturnsAsync(logs);

        // Act
        var result = await _weatherService.GetWeatherLogsAsync(fromTime, toTime);

        // Assert
        Assert.That(result, Is.EqualTo(logs));
    }

    [Test]
    public async Task GetWeatherLogsAsync_ShouldReturnEmptyList_WhenNoLogsExistInRange()
    {
        // Arrange
        var fromTime = DateTime.UtcNow.AddDays(-1);
        var toTime = DateTime.UtcNow;
        var logs = new List<WeatherEntity>();

        _tableStorageServiceMock
            .Setup(ts => ts.GetLogsAsync(fromTime, toTime))
            .ReturnsAsync(logs);

        // Act
        var result = await _weatherService.GetWeatherLogsAsync(fromTime, toTime);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClientFactoryMock = null;
    }
}


public class FakeHttpMessageHandler : DelegatingHandler
{
    public HttpResponseMessage ResponseMessage { get; set; }
    public bool ShouldThrow { get; set; } = false;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (ShouldThrow)
        {
            throw new HttpRequestException("Network error");
        }

        return Task.FromResult(ResponseMessage ?? new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}