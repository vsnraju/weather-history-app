namespace Weather.Application.Weather;

public sealed record WeatherEntryDto(
    string Input,
    string? Date,
    double? MinTemperatureC,
    double? MaxTemperatureC,
    double? PrecipitationMm,
    string Status,
    string? ErrorMessage,
    bool IsCached);
