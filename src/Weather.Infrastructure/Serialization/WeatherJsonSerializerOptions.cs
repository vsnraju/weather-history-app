using System.Text.Json;

namespace Weather.Infrastructure.Serialization;

internal static class WeatherJsonSerializerOptions
{
    public static JsonSerializerOptions Default { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
}
