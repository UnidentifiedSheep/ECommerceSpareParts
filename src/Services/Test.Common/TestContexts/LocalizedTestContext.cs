using Localization.Abstractions.Interfaces;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class LocalizedTestContext(IScopedStringLocalizer localizer) : ITestContext
{
    public virtual Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        localizer.SetLocale("ru-RU");
        return Task.CompletedTask;
    }
}