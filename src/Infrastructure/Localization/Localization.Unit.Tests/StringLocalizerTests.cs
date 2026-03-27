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
        container.Initialize(new Dictionary<string, string>
        {
            [key] = value
        });

        var localizer = new StringLocalizer([container]);

        var result = localizer.Get(key, locale);

        result.Should().Be(value);
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