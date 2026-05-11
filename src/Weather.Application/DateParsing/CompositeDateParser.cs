using Weather.Application.Abstractions.DateParsing;
using Weather.Domain.Common;

namespace Weather.Application.DateParsing;

public sealed class CompositeDateParser(IEnumerable<IDateParsingStrategy> strategies) : IDateParser
{
    private readonly IReadOnlyList<IDateParsingStrategy> _strategies = strategies.ToArray();

    public Result<DateOnly> Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<DateOnly>.Failure(new Error("Date.Empty", "Date value is empty."));
        }

        var trimmedInput = input.Trim();

        foreach (var strategy in _strategies)
        {
            var result = strategy.TryParse(trimmedInput);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        return Result<DateOnly>.Failure(new Error(
            "Date.Invalid",
            $"'{input}' is not a valid supported date. Supported examples: 02/27/2021, June 2, 2022, Jul-13-2020."));
    }
}
