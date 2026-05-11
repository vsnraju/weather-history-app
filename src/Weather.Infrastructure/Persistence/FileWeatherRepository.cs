using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Weather.Application.Abstractions.Persistence;
using Weather.Domain.Common;
using Weather.Domain.Weather;
using Weather.Infrastructure.Configuration;
using Weather.Infrastructure.Serialization;

namespace Weather.Infrastructure.Persistence;

public sealed class FileWeatherRepository(
    IOptions<WeatherStorageOptions> options,
    IHostEnvironment hostEnvironment) : IWeatherRepository
{
    private readonly WeatherStorageOptions _options = options.Value;

    public async Task<Result<WeatherDataPoint?>> GetAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var path = GetFilePath(date);
        if (!File.Exists(path))
        {
            return Result<WeatherDataPoint?>.Success(null);
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var weatherDataPoint = await JsonSerializer.DeserializeAsync<WeatherDataPoint>(
                stream,
                WeatherJsonSerializerOptions.Default,
                cancellationToken);

            return weatherDataPoint is null
                ? Result<WeatherDataPoint?>.Failure(new Error(
                    "WeatherStorage.EmptyFile",
                    $"Weather data file '{path}' did not contain a valid payload."))
                : Result<WeatherDataPoint?>.Success(weatherDataPoint);
        }
        catch (JsonException exception)
        {
            return Result<WeatherDataPoint?>.Failure(new Error(
                "WeatherStorage.InvalidJson",
                $"Weather data file '{path}' contains invalid JSON: {exception.Message}"));
        }
        catch (IOException exception)
        {
            return Result<WeatherDataPoint?>.Failure(new Error(
                "WeatherStorage.ReadFailed",
                $"Could not read weather data file '{path}': {exception.Message}"));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Result<WeatherDataPoint?>.Failure(new Error(
                "WeatherStorage.AccessDenied",
                $"Access to weather data file '{path}' was denied: {exception.Message}"));
        }
    }

    public async Task<Result> SaveAsync(WeatherDataPoint weatherDataPoint, CancellationToken cancellationToken)
    {
        var path = GetFilePath(weatherDataPoint.Date);
        var directory = Path.GetDirectoryName(path)!;
        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";

        try
        {
            Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(weatherDataPoint, WeatherJsonSerializerOptions.Default);
            await File.WriteAllTextAsync(tempPath, json, cancellationToken);
            File.Move(tempPath, path, overwrite: true);

            return Result.Success();
        }
        catch (JsonException exception)
        {
            return Result.Failure(new Error(
                "WeatherStorage.SerializeFailed",
                $"Could not serialize weather data for {weatherDataPoint.Date:yyyy-MM-dd}: {exception.Message}"));
        }
        catch (IOException exception)
        {
            return Result.Failure(new Error(
                "WeatherStorage.WriteFailed",
                $"Could not write weather data file '{path}': {exception.Message}"));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Result.Failure(new Error(
                "WeatherStorage.AccessDenied",
                $"Access to weather data path '{path}' was denied: {exception.Message}"));
        }
        finally
        {
            TryDeleteTempFile(tempPath);
        }
    }

    private string GetFilePath(DateOnly date)
    {
        return Path.Combine(ResolvePath(_options.WeatherDataPath), $"{date:yyyy-MM-dd}.json");
    }

    private string ResolvePath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine(hostEnvironment.ContentRootPath, path);
    }

    private static void TryDeleteTempFile(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
