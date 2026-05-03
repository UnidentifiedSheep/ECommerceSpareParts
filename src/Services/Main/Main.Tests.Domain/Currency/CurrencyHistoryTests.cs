using FluentAssertions;
using Main.Entities.Currency;

namespace Main.Tests.Domain.Currency;

public class CurrencyHistoryTests
{
    [Fact]
    public void Create_SetsValuesAndTimestamp()
    {
        var before = DateTime.UtcNow;

        var history = CurrencyHistory.Create(1, 1.0m, 2.0m);

        var after = DateTime.UtcNow;

        history.CurrencyId.Should().Be(1);
        history.PrevValue.Should().Be(1.0m);
        history.NewValue.Should().Be(2.0m);

        history.Datetime.Should().BeOnOrAfter(before);
        history.Datetime.Should().BeOnOrBefore(after);
    }
}