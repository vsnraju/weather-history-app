using Microsoft.Extensions.DependencyInjection;
using Weather.Application.Abstractions.DateParsing;
using Weather.Application.DateParsing;
using Weather.Application.Weather;

namespace Weather.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IDateParser, DateParser>();
        services.AddScoped<IWeatherReportService, WeatherReportService>();

        return services;
    }
}
