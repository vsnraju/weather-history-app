using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Weather.Application.Abstractions.Input;
using Weather.Domain.Common;
using Weather.Infrastructure.Configuration;

namespace Weather.Infrastructure.Input;

public sealed class FileDateFileReader(
    IOptions<WeatherStorageOptions> options,
    IHostEnvironment hostEnvironment) : IDateFileReader
{
    private readonly WeatherStorageOptions _options = options.Value;

    public async Task<Result<IReadOnlyList<string>>> ReadDatesAsync(CancellationToken cancellationToken)
    {
        var path = ResolvePath(_options.DatesFilePath);

        try
        {
            if (!File.Exists(path))
            {
                return Result<IReadOnlyList<string>>.Failure(new Error(
                    "DatesFile.NotFound",
                    $"The dates file was not found at '{path}'."));
            }

            var lines = await File.ReadAllLinesAsync(path, cancellationToken);
            var dates = lines
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            return Result<IReadOnlyList<string>>.Success(dates);
        }
        catch (IOException exception)
        {
            return Result<IReadOnlyList<string>>.Failure(new Error(
                "DatesFile.ReadFailed",
                $"Could not read the dates file: {exception.Message}"));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Result<IReadOnlyList<string>>.Failure(new Error(
                "DatesFile.AccessDenied",
                $"Access to the dates file was denied: {exception.Message}"));
        }
    }

    private string ResolvePath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine(hostEnvironment.ContentRootPath, path);
    }
}
