using System.Globalization;
using ITestContext = Tests.Interfaces.ITestContext;

namespace Tests.TestContexts;

public class LocalizedTestContext : ITestContext
{
	public virtual Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("ru-RU");
		return Task.CompletedTask;
	}
}
