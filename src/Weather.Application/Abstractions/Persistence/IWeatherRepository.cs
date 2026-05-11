using Weather.Domain.Common;
using Weather.Domain.Weather;

namespace Weather.Application.Abstractions.Persistence;

public interface IWeatherRepository
{
    Task<Result<WeatherDataPoint?>> GetAsync(DateOnly date, CancellationToken cancellationToken);

    Task<Result> SaveAsync(WeatherDataPoint weatherDataPoint, CancellationToken cancellationToken);
}
