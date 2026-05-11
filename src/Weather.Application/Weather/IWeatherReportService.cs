using Weather.Domain.Common;

namespace Weather.Application.Weather;

public interface IWeatherReportService
{
    Task<Result<IReadOnlyList<WeatherEntryDto>>> GetWeatherAsync(CancellationToken cancellationToken);
}
