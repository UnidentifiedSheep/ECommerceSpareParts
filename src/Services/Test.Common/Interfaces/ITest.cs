namespace Test.Common.Interfaces;

public interface ITest
{
    Task InitializeAsync();
    Task DisposeAsync();
    void RegisterBasicContext<TContext>() where TContext : class, ITestContext;
    void RemoveBasicContext<TContext>() where TContext : class, ITestContext;
}