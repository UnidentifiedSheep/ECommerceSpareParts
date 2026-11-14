using Application.Common.Interfaces;
using Pricing.Application.Interfaces.Markup;

namespace Pricing.Application.Startup;

public class MarkupInitializationStartupTask(IMarkupInitializer markupInitializer) : IStartupTask
{
	public Task ExecuteAsync(CancellationToken ct) => markupInitializer.Initialize(ct);
}
