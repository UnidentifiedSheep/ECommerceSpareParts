namespace Test.Common.Interfaces;

public interface ITestContext
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}