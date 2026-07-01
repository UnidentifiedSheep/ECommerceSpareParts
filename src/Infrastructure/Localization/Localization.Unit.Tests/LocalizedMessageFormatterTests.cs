using FluentAssertions;
using Localization.Abstractions;

namespace Localization.Unit.Tests;

public class LocalizedMessageFormatterTests
{
    [Fact]
    public void TryFormat_ShouldReturnTemplate_WhenArgumentsAreNull()
    {
        var success = LocalizedMessageFormatter.TryFormat(
            "Message",
            null,
            out var result);

        success.Should().BeTrue();
        result.Should().Be("Message");
    }

    [Fact]
    public void TryFormat_ShouldFormatTemplate_WhenArgumentsProvided()
    {
        var success = LocalizedMessageFormatter.TryFormat(
            "Value '{0}' must be greater than {1}.",
            ["Count", 0],
            out var result);

        success.Should().BeTrue();
        result.Should().Be("Value 'Count' must be greater than 0.");
    }

    [Fact]
    public void TryFormat_ShouldReturnFalseAndAppendMarker_WhenTemplateIsInvalid()
    {
        var success = LocalizedMessageFormatter.TryFormat(
            "Value '{0}' must be greater than {1}.",
            ["Count"],
            out var result);

        success.Should().BeFalse();
        result.Should().Be("Value '{0}' must be greater than {1}. [Error formatting message]");
    }
}