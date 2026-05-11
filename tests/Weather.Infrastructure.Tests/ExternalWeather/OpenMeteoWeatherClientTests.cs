using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Weather.Infrastructure.Configuration;
using Weather.Infrastructure.ExternalWeather.OpenMeteo;

namespace Weather.Infrastructure.Tests.ExternalWeather;

public sealed class OpenMeteoWeatherClientTests
{
    [Fact]
    public async Task GetHistoricalWeatherAsync_MapsDailyWeather_WhenResponseContainsRequestedDate()
    {
        const string json = """
            {
              "daily": {
                "time": [ "2021-02-27" ],
                "temperature_2m_min": [ -3.4 ],
                "temperature_2m_max": [ 4.8 ],
                "precipitation_sum": [ 1.2 ]
              }
            }
            """;
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, json);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://archive-api.open-meteo.test")
        };
        var client = new OpenMeteoWeatherClient(
            httpClient,
            Options.Create(new WeatherApiOptions()),
            Mock.Of<ILogger<OpenMeteoWeatherClient>>());

        var result = await client.GetHistoricalWeatherAsync(new DateOnly(2021, 2, 27), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.MinTemperatureC.Should().Be(-3.4);
        result.Value.MaxTemperatureC.Should().Be(4.8);
        result.Value.PrecipitationMm.Should().Be(1.2);
        handler.RequestUri!.Query.Should().Contain("start_date=2021-02-27");
        handler.RequestUri.Query.Should().Contain("end_date=2021-02-27");
    }

    [Fact]
    public async Task GetHistoricalWeatherAsync_ReturnsFailure_WhenDailyDataIsMissing()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{ "daily": null }""");
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://archive-api.open-meteo.test")
        };
        var client = new OpenMeteoWeatherClient(
            httpClient,
            Options.Create(new WeatherApiOptions()),
            Mock.Of<ILogger<OpenMeteoWeatherClient>>());

        var result = await client.GetHistoricalWeatherAsync(new DateOnly(2021, 2, 27), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("OpenMeteo.EmptyDailyData");
    }
}
