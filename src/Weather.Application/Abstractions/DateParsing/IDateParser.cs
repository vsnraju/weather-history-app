using Weather.Domain.Common;

namespace Weather.Application.Abstractions.DateParsing;

public interface IDateParser
{
    Result<DateOnly> Parse(string input);
}
