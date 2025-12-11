using Core.Abstractions;

namespace Exceptions.Base.Examples;

public class ConcurrencyCodeMismatchExample : BaseExceptionExample
{
    public string? ClientCode { get; }
    public string? ServerCode { get; }

    public ConcurrencyCodeMismatchExample()
    {
        Detail = "Concurrency code mismatch.";
        ClientCode = "123";
        ServerCode = "456";
    }
}