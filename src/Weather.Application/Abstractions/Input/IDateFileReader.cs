using Weather.Domain.Common;

namespace Weather.Application.Abstractions.Input;

public interface IDateFileReader
{
    Task<Result<IReadOnlyList<string>>> ReadDatesAsync(CancellationToken cancellationToken);
}
