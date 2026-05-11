using FluentAssertions;
using Weather.Application.DateParsing;

namespace Weather.Application.Tests.DateParsing;

public sealed class DateParserTests
{
    [Theory]
    [InlineData("02/27/2021", "2021-02-27")]
    [InlineData("June 2, 2022", "2022-06-02")]
    [InlineData("Jul-13-2020", "2020-07-13")]
    public void Parse_ReturnsDate_WhenInputMatchesSupportedFormat(string input, string expectedIsoDate)
    {
        var parser = new DateParser();

        var result = parser.Parse(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.ToString("yyyy-MM-dd").Should().Be(expectedIsoDate);
    }

    [Fact]
    public void Parse_ReturnsFailure_WhenCalendarDateIsInvalid()
    {
        var parser = new DateParser();

        var result = parser.Parse("April 31, 2022");

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Date.Invalid");
    }

    [Fact]
    public void Parse_ReturnsFailure_WhenDateIsEmpty()
    {
        var parser = new DateParser();

        var result = parser.Parse(" ");

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Date.Empty");
    }
}
