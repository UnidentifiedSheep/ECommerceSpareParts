using FluentAssertions;
using Localization.Domain;

namespace Localization.Unit.Tests;

public class StringLocalizerTests
{
    [Fact]
    public void Get_ShouldReturnValue_WhenKeyExists()
    {
        var locale = "en";
        var key = "Test.Key";
        var value = "Test.Value";
        var container = new LocalizerContainer(locale);
        container.Initialize(
            new Dictionary<string, string>
            {
                [key] = value
            });

        var localizer = new StringLocalizer([container]);

        var result = localizer.Get(key, locale);

        result.Should().Be(value);
    }

    [Fact]
    public void Get_ShouldReturnFormattedValue_WhenArgumentsProvided()
    {
        var container = new LocalizerContainer("en");
        container.Initialize(
            new Dictionary<string, string>
            {
                ["Test.Key"] = "Hello, {0}."
            });

        var localizer = new StringLocalizer([container]);

        var result = localizer.Get(
            "Test.Key",
            "en",
            "World");

        result.Should().Be("Hello, World.");
    }

    [Fact]
    public void TryGet_ShouldReturnFormattedValue_WhenArgumentsProvided()
    {
        var container = new LocalizerContainer("en");
        container.Initialize(
            new Dictionary<string, string>
            {
                ["Test.Key"] = "Hello, {0}."
            });

        var localizer = new StringLocalizer([container]);

        var success = localizer.TryGet(
            "Test.Key",
            "en",
            out var result,
            "World");

        success.Should().BeTrue();
        result.Should().Be("Hello, World.");
    }

    [Fact]
    public void TryGet_ShouldReturnFalse_WhenFormattingFailed()
    {
        var container = new LocalizerContainer("en");
        container.Initialize(
            new Dictionary<string, string>
            {
                ["Test.Key"] = "Hello, {0} {1}."
            });

        var localizer = new StringLocalizer([container]);

        var success = localizer.TryGet(
            "Test.Key",
            "en",
            out var result,
            "World");

        success.Should().BeFalse();
        result.Should().Be("Hello, {0} {1}. [Error formatting message]");
    }

    [Fact]
    public void Get_ShouldThrow_WhenLocaleNotFound()
    {
        var container = new LocalizerContainer("en");
        container.Initialize(new Dictionary<string, string>());

        var localizer = new StringLocalizer(new[] { container });

        Assert.Throws<InvalidOperationException>(() =>
            localizer.Get("key", "de"));
    }

    [Fact]
    public void Get_ShouldThrow_WhenKeyNotFound()
    {
        var container = new LocalizerContainer("en");
        container.Initialize(new Dictionary<string, string>());

        var localizer = new StringLocalizer(new[] { container });

        Assert.Throws<InvalidOperationException>(() =>
            localizer.Get("missing", "en"));
    }
}