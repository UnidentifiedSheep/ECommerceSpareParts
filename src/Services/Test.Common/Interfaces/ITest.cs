namespace Tests.Interfaces;

public interface ITest
{
	void RegisterBasicContext<TContext>() where TContext : class, ITestContext;

	void RemoveBasicContext<TContext>() where TContext : class, ITestContext;
}
