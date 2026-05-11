using Weather.Domain.Common;

namespace Weather.Application.Abstractions.DateParsing;

public interface IDateParsingStrategy
{
    string Name { get; }

    Result<DateOnly> TryParse(string input);
}
