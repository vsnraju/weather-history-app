using System.Globalization;
using Weather.Application.Abstractions.DateParsing;
using Weather.Domain.Common;

namespace Weather.Application.DateParsing;

public sealed class DateParser : IDateParser
{
    private static readonly string[] SupportedFormats =
    [
        "MM/dd/yyyy",
        "MMMM d, yyyy",
        "MMM-dd-yyyy"
    ];

    public Result<DateOnly> Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<DateOnly>.Failure(new Error("Date.Empty", "Date value is empty."));
        }

        return DateTime.TryParseExact(
            input.Trim(),
            SupportedFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedDate)
            ? Result<DateOnly>.Success(DateOnly.FromDateTime(parsedDate))
            : Result<DateOnly>.Failure(new Error(
                "Date.Invalid",
                $"'{input}' is not a valid supported date. Supported examples: 02/27/2021, June 2, 2022, Jul-13-2020."));
    }
}
