using System.Text.Json.Serialization;

namespace Weather.Infrastructure.ExternalWeather.OpenMeteo;

internal sealed record OpenMeteoArchiveResponse(
    [property: JsonPropertyName("daily")] OpenMeteoDaily? Daily);

internal sealed record OpenMeteoDaily(
    [property: JsonPropertyName("time")] DateOnly[]? Time,
    [property: JsonPropertyName("temperature_2m_min")] double?[]? Temperature2mMin,
    [property: JsonPropertyName("temperature_2m_max")] double?[]? Temperature2mMax,
    [property: JsonPropertyName("precipitation_sum")] double?[]? PrecipitationSum);
