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
    
    [Fact]
    public void TryGet_ShouldThrow_WhenLocaleNotSet()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = new ScopedStringLocalizer(baseLocalizer);

        var result = () => scoped.TryGet("Test.Key", out _);
        result.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryGet_ShouldReturnTrueAndValue_WhenKeyExists()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = new ScopedStringLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var success = scoped.TryGet("Test.Key", out var value);

        success.Should().BeTrue();
        value.Should().Be("value");
    }

    [Fact]
    public void TryGet_ShouldReturnFalseAndNull_WhenKeyDoesNotExist()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = new ScopedStringLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var success = scoped.TryGet("NonExistent.Key", out var value);

        success.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryGet_ShouldThrow_WhenDisposed()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = new ScopedStringLocalizer(baseLocalizer);

        scoped.Dispose();

        var result = () => scoped.TryGet("Test.Key", out _);
        result.Should().Throw<ObjectDisposedException>();
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