using System.Globalization;
using Weather.Application.Abstractions.DateParsing;
using Weather.Domain.Common;

namespace Weather.Application.DateParsing;

public sealed class LongMonthDateParsingStrategy : IDateParsingStrategy
{
    private const string Format = "MMMM d, yyyy";

    public string Name => "Long month";

    public Result<DateOnly> TryParse(string input)
    {
        return DateOnly.TryParseExact(
            input,
            Format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date)
            ? Result<DateOnly>.Success(date)
            : Result<DateOnly>.Failure(new Error("Date.InvalidFormat", $"'{input}' does not match {Format}."));
    }
}
