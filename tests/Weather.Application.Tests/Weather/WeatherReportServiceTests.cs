using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Weather.Application.Abstractions.DateParsing;
using Weather.Application.Abstractions.ExternalWeather;
using Weather.Application.Abstractions.Input;
using Weather.Application.Abstractions.Persistence;
using Weather.Application.DateParsing;
using Weather.Application.Weather;
using Weather.Domain.Common;
using Weather.Domain.Weather;

namespace Weather.Application.Tests.Weather;

public sealed class WeatherReportServiceTests
{
    private readonly Mock<IDateFileReader> _dateFileReader = new();
    private readonly IDateParser _dateParser = new DateParser();
    private readonly Mock<IWeatherRepository> _weatherRepository = new();
    private readonly Mock<IWeatherApiClient> _weatherApiClient = new();

    [Fact]
    public async Task GetWeatherAsync_AddsInvalidDateEntry_AndDoesNotCallExternalApi()
    {
        _dateFileReader
            .Setup(reader => reader.ReadDatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<string>>.Success(["April 31, 2022"]));

        var service = CreateService();

        var result = await service.GetWeatherAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(entry =>
            entry.Input == "April 31, 2022" &&
            entry.Status == "InvalidDate" &&
            entry.Date == null);
        _weatherApiClient.Verify(
            client => client.GetHistoricalWeatherAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetWeatherAsync_UsesCachedWeather_WithoutCallingExternalApiOrSaving()
    {
        var date = new DateOnly(2021, 2, 27);
        var cachedWeather = WeatherDataPoint.Create(date, 1.2, 9.8, 0.4).Value;

        _dateFileReader
            .Setup(reader => reader.ReadDatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<string>>.Success(["02/27/2021"]));
        _weatherRepository
            .Setup(repository => repository.GetAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WeatherDataPoint?>.Success(cachedWeather));

        var service = CreateService();

        var result = await service.GetWeatherAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(entry =>
            entry.Status == "Cached" &&
            entry.IsCached &&
            entry.MinTemperatureC == cachedWeather.MinTemperatureC);
        _weatherApiClient.Verify(
            client => client.GetHistoricalWeatherAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _weatherRepository.Verify(
            repository => repository.SaveAsync(It.IsAny<WeatherDataPoint>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetWeatherAsync_FetchesAndSavesWeather_WhenCacheMisses()
    {
        var date = new DateOnly(2022, 6, 2);
        var fetchedWeather = WeatherDataPoint.Create(date, 20.1, 31.4, 2.5).Value;

        _dateFileReader
            .Setup(reader => reader.ReadDatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<string>>.Success(["June 2, 2022"]));
        _weatherRepository
            .Setup(repository => repository.GetAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WeatherDataPoint?>.Success(null));
        _weatherApiClient
            .Setup(client => client.GetHistoricalWeatherAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WeatherDataPoint>.Success(fetchedWeather));
        _weatherRepository
            .Setup(repository => repository.SaveAsync(fetchedWeather, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var service = CreateService();

        var result = await service.GetWeatherAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(entry =>
            entry.Status == "Fetched" &&
            !entry.IsCached &&
            entry.Date == "2022-06-02");
        _weatherRepository.Verify(
            repository => repository.SaveAsync(fetchedWeather, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private WeatherReportService CreateService()
    {
        return new WeatherReportService(
            _dateFileReader.Object,
            _dateParser,
            _weatherRepository.Object,
            _weatherApiClient.Object,
            Mock.Of<ILogger<WeatherReportService>>());
    }
}
