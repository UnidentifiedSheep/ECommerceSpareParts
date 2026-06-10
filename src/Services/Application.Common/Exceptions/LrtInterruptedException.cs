namespace Application.Common.Exceptions;

public sealed class LrtInterruptedException(string reason)
    : Exception(reason)
{
    public string Reason { get; } = reason;
}