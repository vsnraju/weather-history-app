using Weather.Domain.Common;

namespace Weather.Domain.Weather;

public sealed record WeatherDataPoint(
    DateOnly Date,
    double MinTemperatureC,
    double MaxTemperatureC,
    double PrecipitationMm)
{
    public static Result<WeatherDataPoint> Create(
        DateOnly date,
        double? minTemperatureC,
        double? maxTemperatureC,
        double? precipitationMm)
    {
        var errors = new List<Error>();

        if (minTemperatureC is null)
        {
            errors.Add(new Error("Weather.MinTemperatureMissing", "Minimum temperature is missing."));
        }

        if (maxTemperatureC is null)
        {
            errors.Add(new Error("Weather.MaxTemperatureMissing", "Maximum temperature is missing."));
        }

        if (precipitationMm is null)
        {
            errors.Add(new Error("Weather.PrecipitationMissing", "Precipitation sum is missing."));
        }
        else if (precipitationMm.Value < 0)
        {
            errors.Add(new Error("Weather.InvalidPrecipitation", "Precipitation sum cannot be negative."));
        }

        if (minTemperatureC.HasValue &&
            maxTemperatureC.HasValue &&
            minTemperatureC.Value > maxTemperatureC.Value)
        {
            errors.Add(new Error("Weather.InvalidTemperatureRange", "Minimum temperature cannot exceed maximum temperature."));
        }

        return errors.Count > 0
            ? Result<WeatherDataPoint>.Failure(errors)
            : Result<WeatherDataPoint>.Success(new WeatherDataPoint(
                date,
                minTemperatureC!.Value,
                maxTemperatureC!.Value,
                precipitationMm!.Value));
    }
}
