namespace Weather.Domain.Common;

public sealed record Error(string Code, string Message)
{
    public static Error None { get; } = new(string.Empty, string.Empty);
}
