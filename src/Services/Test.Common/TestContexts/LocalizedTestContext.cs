using Abstractions.Interfaces.Tests;
using Localization.Abstractions.Interfaces;

namespace Test.Common.TestContexts;

public class LocalizedTestContext(IScopedStringLocalizer localizer) : ITestContext
{
    public virtual Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        localizer.SetLocale("ru-RU");
        return Task.CompletedTask;
    }
}