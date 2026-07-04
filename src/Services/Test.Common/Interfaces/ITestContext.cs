namespace Tests.Interfaces;

public interface ITestContext
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}