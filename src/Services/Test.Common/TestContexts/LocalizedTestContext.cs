using Localization.Abstractions.Interfaces;
using Test.Common.Interfaces;

namespace Test.Common.TestContexts;

public class LocalizedTestContext(IScopedStringLocalizer localizer) : ITestContext
{
    public virtual Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        localizer.SetLocale("ru-RU");
        return Task.CompletedTask;
    }
}