using Microsoft.Extensions.Logging;
using Weather.Application.Abstractions.DateParsing;
using Weather.Application.Abstractions.ExternalWeather;
using Weather.Application.Abstractions.Input;
using Weather.Application.Abstractions.Persistence;
using Weather.Domain.Common;
using Weather.Domain.Weather;

namespace Weather.Application.Weather;

public sealed class WeatherReportService(
    IDateFileReader dateFileReader,
    IDateParser dateParser,
    IWeatherRepository weatherRepository,
    IWeatherApiClient weatherApiClient,
    ILogger<WeatherReportService> logger) : IWeatherReportService
{
    public async Task<Result<IReadOnlyList<WeatherEntryDto>>> GetWeatherAsync(CancellationToken cancellationToken)
    {
        var readResult = await dateFileReader.ReadDatesAsync(cancellationToken);
        if (readResult.IsFailure)
        {
            return Result<IReadOnlyList<WeatherEntryDto>>.Failure(readResult.Errors);
        }

        var entries = new List<WeatherEntryDto>();

        foreach (var input in readResult.Value)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var parseResult = dateParser.Parse(input);
            if (parseResult.IsFailure)
            {
                entries.Add(new WeatherEntryDto(
                    input,
                    null,
                    null,
                    null,
                    null,
                    "InvalidDate",
                    parseResult.FirstError.Message,
                    false));
                continue;
            }

            var date = parseResult.Value;
            var cacheResult = await weatherRepository.GetAsync(date, cancellationToken);
            if (cacheResult.IsSuccess && cacheResult.Value is not null)
            {
                entries.Add(ToDto(input, cacheResult.Value, "Cached", null, true));
                continue;
            }

            if (cacheResult.IsFailure)
            {
                logger.LogWarning(
                    "Weather cache read failed for {Date}: {Error}",
                    date,
                    cacheResult.FirstError.Message);
            }

            var weatherResult = await weatherApiClient.GetHistoricalWeatherAsync(date, cancellationToken);
            if (weatherResult.IsFailure)
            {
                entries.Add(new WeatherEntryDto(
                    input,
                    ToIsoDate(date),
                    null,
                    null,
                    null,
                    "WeatherUnavailable",
                    weatherResult.FirstError.Message,
                    false));
                continue;
            }

            var saveResult = await weatherRepository.SaveAsync(weatherResult.Value, cancellationToken);
            entries.Add(saveResult.IsSuccess
                ? ToDto(input, weatherResult.Value, "Fetched", null, false)
                : ToDto(input, weatherResult.Value, "StorageFailed", saveResult.FirstError.Message, false));
        }

        return Result<IReadOnlyList<WeatherEntryDto>>.Success(entries);
    }

    private static WeatherEntryDto ToDto(
        string input,
        WeatherDataPoint weatherDataPoint,
        string status,
        string? errorMessage,
        bool isCached)
    {
        return new WeatherEntryDto(
            input,
            ToIsoDate(weatherDataPoint.Date),
            weatherDataPoint.MinTemperatureC,
            weatherDataPoint.MaxTemperatureC,
            weatherDataPoint.PrecipitationMm,
            status,
            errorMessage,
            isCached);
    }

    private static string ToIsoDate(DateOnly date) => date.ToString("yyyy-MM-dd");
}
