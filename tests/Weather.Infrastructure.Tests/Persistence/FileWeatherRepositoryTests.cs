using FluentAssertions;
using Microsoft.Extensions.Options;
using Weather.Domain.Weather;
using Weather.Infrastructure.Configuration;
using Weather.Infrastructure.Persistence;
using Weather.Infrastructure.Tests.TestSupport;

namespace Weather.Infrastructure.Tests.Persistence;

public sealed class FileWeatherRepositoryTests
{
    [Fact]
    public async Task SaveAsync_ThenGetAsync_RoundTripsWeatherData()
    {
        using var directory = TemporaryDirectory.Create();
        var repository = new FileWeatherRepository(
            Options.Create(new WeatherStorageOptions { WeatherDataPath = "weather-data" }),
            TestHostEnvironment.ForContentRoot(directory.Path));
        var weatherData = WeatherDataPoint.Create(new DateOnly(2021, 2, 27), -3.4, 4.8, 1.2).Value;

        var saveResult = await repository.SaveAsync(weatherData, CancellationToken.None);
        var readResult = await repository.GetAsync(weatherData.Date, CancellationToken.None);

        saveResult.IsSuccess.Should().BeTrue();
        readResult.IsSuccess.Should().BeTrue();
        readResult.Value.Should().BeEquivalentTo(weatherData);
        File.Exists(Path.Combine(directory.Path, "weather-data", "2021-02-27.json")).Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_ReturnsNullSuccess_WhenFileDoesNotExist()
    {
        using var directory = TemporaryDirectory.Create();
        var repository = new FileWeatherRepository(
            Options.Create(new WeatherStorageOptions { WeatherDataPath = "weather-data" }),
            TestHostEnvironment.ForContentRoot(directory.Path));

        var result = await repository.GetAsync(new DateOnly(2030, 1, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
