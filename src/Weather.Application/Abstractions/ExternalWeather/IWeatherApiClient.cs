using Weather.Domain.Common;
using Weather.Domain.Weather;

namespace Weather.Application.Abstractions.ExternalWeather;

public interface IWeatherApiClient
{
    Task<Result<WeatherDataPoint>> GetHistoricalWeatherAsync(DateOnly date, CancellationToken cancellationToken);
}
