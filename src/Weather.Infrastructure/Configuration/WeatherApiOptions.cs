namespace Weather.Infrastructure.Configuration;

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string BaseUrl { get; init; } = "https://archive-api.open-meteo.com";

    public double Latitude { get; init; } = 32.78;

    public double Longitude { get; init; } = -96.8;

    public string DailyVariables { get; init; } = "temperature_2m_min,temperature_2m_max,precipitation_sum";

    public string Timezone { get; init; } = "auto";

    public int TimeoutSeconds { get; init; } = 20;
}
