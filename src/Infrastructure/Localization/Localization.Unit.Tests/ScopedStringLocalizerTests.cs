using FluentAssertions;
using Localization.Domain;
using Microsoft.Extensions.Options;

namespace Localization.Unit.Tests;

public class ScopedStringLocalizerTests
{
    [Fact]
    public void Get_ShouldUseDefaultLocale_WhenLocaleNotSet()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        var result = scoped.Get("Test.Key");

        result.Should().Be("value");
    }

    [Fact]
    public void Get_ShouldReturnValue_WhenLocaleSet()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var result = scoped.Get("Test.Key");

        result.Should().Be("value");
    }

    [Fact]
    public void Get_ShouldReturnFormattedValue_WhenArgumentsProvided()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var result = scoped.Get("Formatted.Key", "World");

        result.Should().Be("Hello, World.");
    }

    [Fact]
    public void Indexer_ShouldWork()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var result = scoped["Test.Key"];

        result.Should().Be("value");
    }

    [Fact]
    public void ShouldThrow_WhenDisposed()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        scoped.Dispose();

        var action = () => scoped.Get("Test.Key");
        action.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void TryGet_ShouldUseDefaultLocale_WhenLocaleNotSet()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        var result = scoped.TryGet("Test.Key", out var value);

        result.Should().BeTrue();
        value.Should().Be("value");
    }

    [Fact]
    public void TryGet_ShouldReturnTrueAndValue_WhenKeyExists()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var success = scoped.TryGet("Test.Key", out var value);

        success.Should().BeTrue();
        value.Should().Be("value");
    }

    [Fact]
    public void TryGet_ShouldReturnTrueAndFormattedValue_WhenArgumentsProvided()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var success = scoped.TryGet(
            "Formatted.Key",
            out var value,
            "World");

        success.Should().BeTrue();
        value.Should().Be("Hello, World.");
    }

    [Fact]
    public void GetOrDefault_ShouldReturnFormattedValue_WhenArgumentsProvided()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var value = scoped.GetOrDefault("Formatted.Key", "World");

        value.Should().Be("Hello, World.");
    }

    [Fact]
    public void TryGet_ShouldReturnFalseAndNull_WhenKeyDoesNotExist()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        scoped.SetLocale("en");

        var success = scoped.TryGet("NonExistent.Key", out var value);

        success.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryGet_ShouldThrow_WhenDisposed()
    {
        var baseLocalizer = CreateBaseLocalizer();
        var scoped = CreateScopedLocalizer(baseLocalizer);

        scoped.Dispose();

        var result = () => scoped.TryGet("Test.Key", out _);
        result.Should().Throw<ObjectDisposedException>();
    }

    private static StringLocalizer CreateBaseLocalizer()
    {
        var container = new LocalizerContainer("en");
        container.Initialize(
            new Dictionary<string, string>
            {
                ["Test.Key"] = "value",
                ["Formatted.Key"] = "Hello, {0}."
            });

        return new StringLocalizer([container]);
    }

    private static ScopedStringLocalizer CreateScopedLocalizer(
        StringLocalizer baseLocalizer)
    {
        return new ScopedStringLocalizer(
            baseLocalizer,
            Options.Create(new LocalesOptions
            {
                Default = "en",
                Supported = ["en"]
            }));
    }
}
