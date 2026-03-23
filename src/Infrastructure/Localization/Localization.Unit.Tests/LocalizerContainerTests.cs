using FluentAssertions;
using Localization.Domain;

namespace Localization.Unit.Tests;

public class LocalizerContainerTests
{
    [Fact]
    public void Initialize_ShouldSetValues()
    {
        var container = new LocalizerContainer("en");

        container.Initialize(new Dictionary<string, string>
        {
            ["key"] = "value"
        });

        container.KetMessages["key"].Should().Be("value");
    }

    [Fact]
    public void Initialize_ShouldThrow_WhenCalledTwice()
    {
        var container = new LocalizerContainer("en");

        container.Initialize(new Dictionary<string, string>());

        var action = () => container.Initialize(new Dictionary<string, string>());
        action.Should().Throw<InvalidOperationException>();
    }
}