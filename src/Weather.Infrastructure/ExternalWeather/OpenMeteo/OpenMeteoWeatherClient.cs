using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weather.Application.Abstractions.ExternalWeather;
using Weather.Domain.Common;
using Weather.Domain.Weather;
using Weather.Infrastructure.Configuration;
using Weather.Infrastructure.Serialization;

namespace Weather.Infrastructure.ExternalWeather.OpenMeteo;

public sealed class OpenMeteoWeatherClient(
    HttpClient httpClient,
    IOptions<WeatherApiOptions> options,
    ILogger<OpenMeteoWeatherClient> logger) : IWeatherApiClient
{
    private readonly WeatherApiOptions _options = options.Value;

    public async Task<Result<WeatherDataPoint>> GetHistoricalWeatherAsync(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(BuildRequestUri(date), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<WeatherDataPoint>.Failure(new Error(
                    "OpenMeteo.HttpFailure",
                    $"Open-Meteo returned {(int)response.StatusCode} {response.StatusCode} for {date:yyyy-MM-dd}."));
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var archiveResponse = await JsonSerializer.DeserializeAsync<OpenMeteoArchiveResponse>(
                stream,
                WeatherJsonSerializerOptions.Default,
                cancellationToken);

            return CreateWeatherDataPoint(date, archiveResponse);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Open-Meteo returned invalid JSON for {Date}.", date);

            return Result<WeatherDataPoint>.Failure(new Error(
                "OpenMeteo.InvalidJson",
                $"Open-Meteo returned invalid JSON for {date:yyyy-MM-dd}."));
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Open-Meteo request failed for {Date}.", date);

            return Result<WeatherDataPoint>.Failure(new Error(
                "OpenMeteo.RequestFailed",
                $"Open-Meteo request failed for {date:yyyy-MM-dd}: {exception.Message}"));
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Open-Meteo request timed out for {Date}.", date);

            return Result<WeatherDataPoint>.Failure(new Error(
                "OpenMeteo.Timeout",
                $"Open-Meteo request timed out for {date:yyyy-MM-dd}."));
        }
    }

    private Result<WeatherDataPoint> CreateWeatherDataPoint(
        DateOnly requestedDate,
        OpenMeteoArchiveResponse? archiveResponse)
    {
        var daily = archiveResponse?.Daily;
        if (daily?.Time is null || daily.Time.Length == 0)
        {
            return Result<WeatherDataPoint>.Failure(new Error(
                "OpenMeteo.EmptyDailyData",
                $"Open-Meteo returned no daily data for {requestedDate:yyyy-MM-dd}."));
        }

        var index = Array.FindIndex(daily.Time, date => date == requestedDate);
        if (index < 0)
        {
            return Result<WeatherDataPoint>.Failure(new Error(
                "OpenMeteo.DateMissing",
                $"Open-Meteo did not include {requestedDate:yyyy-MM-dd} in the daily data."));
        }

        return WeatherDataPoint.Create(
            requestedDate,
            GetAt(daily.Temperature2mMin, index),
            GetAt(daily.Temperature2mMax, index),
            GetAt(daily.PrecipitationSum, index));
    }

    private string BuildRequestUri(DateOnly date)
    {
        var isoDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var query = new Dictionary<string, string>
        {
            ["latitude"] = _options.Latitude.ToString(CultureInfo.InvariantCulture),
            ["longitude"] = _options.Longitude.ToString(CultureInfo.InvariantCulture),
            ["start_date"] = isoDate,
            ["end_date"] = isoDate,
            ["daily"] = _options.DailyVariables,
            ["timezone"] = _options.Timezone
        };

        var encodedQuery = string.Join(
            "&",
            query.Select(pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}"));

        return $"/v1/archive?{encodedQuery}";
    }

    private static double? GetAt(double?[]? values, int index)
    {
        return values is not null && index >= 0 && index < values.Length
            ? values[index]
            : null;
    }
}
