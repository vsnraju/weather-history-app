using FluentAssertions;
using Microsoft.Extensions.Options;
using Weather.Infrastructure.Configuration;
using Weather.Infrastructure.Input;
using Weather.Infrastructure.Tests.TestSupport;

namespace Weather.Infrastructure.Tests.Input;

public sealed class FileDateFileReaderTests
{
    [Fact]
    public async Task ReadDatesAsync_ReturnsTrimmedNonEmptyLines()
    {
        using var directory = TemporaryDirectory.Create();
        await File.WriteAllLinesAsync(
            Path.Combine(directory.Path, "dates.txt"),
            [" 02/27/2021 ", "", "June 2, 2022"]);
        var reader = new FileDateFileReader(
            Options.Create(new WeatherStorageOptions { DatesFilePath = "dates.txt" }),
            TestHostEnvironment.ForContentRoot(directory.Path));

        var result = await reader.ReadDatesAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal("02/27/2021", "June 2, 2022");
    }
}
