namespace Abstractions.Interfaces.Tests;

public interface ITestContext
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}