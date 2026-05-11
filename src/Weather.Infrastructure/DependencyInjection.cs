using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Weather.Application.Abstractions.ExternalWeather;
using Weather.Application.Abstractions.Input;
using Weather.Application.Abstractions.Persistence;
using Weather.Infrastructure.Configuration;
using Weather.Infrastructure.ExternalWeather.OpenMeteo;
using Weather.Infrastructure.Input;
using Weather.Infrastructure.Persistence;

namespace Weather.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<WeatherApiOptions>()
            .Bind(configuration.GetSection(WeatherApiOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "Weather API base URL must be absolute.")
            .Validate(options => options.TimeoutSeconds > 0, "Weather API timeout must be positive.")
            .ValidateOnStart();

        services
            .AddOptions<WeatherStorageOptions>()
            .Bind(configuration.GetSection(WeatherStorageOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatesFilePath), "Dates file path is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.WeatherDataPath), "Weather data path is required.")
            .ValidateOnStart();

        services.AddSingleton<IDateFileReader, FileDateFileReader>();
        services.AddSingleton<IWeatherRepository, FileWeatherRepository>();

        services
            .AddHttpClient<IWeatherApiClient, OpenMeteoWeatherClient>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<WeatherApiOptions>>().Value;

                httpClient.BaseAddress = new Uri(options.BaseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler(CreateRetryPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(250 * Math.Pow(2, retryAttempt)));
    }
}
