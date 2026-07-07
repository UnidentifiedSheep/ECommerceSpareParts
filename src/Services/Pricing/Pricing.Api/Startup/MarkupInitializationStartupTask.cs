using Application.Common.Interfaces;
using Pricing.Application.Interfaces.Markup;

namespace Pricing.Api.Startup;

public class MarkupInitializationStartupTask(
    IMarkupInitializer markupInitializer) : IStartupTask
{
    public Task ExecuteAsync(CancellationToken ct) => markupInitializer.Initialize(ct);
}