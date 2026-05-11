namespace Weather.Infrastructure.Configuration;

public sealed class WeatherStorageOptions
{
    public const string SectionName = "WeatherStorage";

    public string DatesFilePath { get; init; } = "dates.txt";

    public string WeatherDataPath { get; init; } = "weather-data";
}
