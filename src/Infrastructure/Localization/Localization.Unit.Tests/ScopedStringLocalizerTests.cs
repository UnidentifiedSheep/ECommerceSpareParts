using FluentAssertions;
using Localization.Domain;

namespace Localization.Unit.Tests;

public class ScopedStringLocalizerTests
{
    [Fact]
    public void Get_ShouldThrow_WhenLocaleNotSet()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = new ScopedStringLocalizer(baseLocalizer);

        var action = () => scoped.Get("key");
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Get_ShouldReturnValue_WhenLocaleSet()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = new ScopedStringLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var result = scoped.Get("Test.Key");

        result.Should().Be("value");
    }

    [Fact]
    public void Indexer_ShouldWork()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = new ScopedStringLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var result = scoped["Test.Key"];

        result.Should().Be("value");
    }

    [Fact]
    public void ShouldThrow_WhenDisposed()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = new ScopedStringLocalizer(baseLocalizer);

        scoped.Dispose();

        var action = () => scoped.Get("Test.Key");
        action.Should().Throw<ObjectDisposedException>();
    }

    private static StringLocalizer CreateBaseLocalizer()
    {
        var container = new LocalizerContainer("en");
        container.Initialize(new Dictionary<string, string>
        {
            ["Test.Key"] = "value"
        });

        return new StringLocalizer([container]);
    }
}