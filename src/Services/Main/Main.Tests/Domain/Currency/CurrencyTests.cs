using FluentAssertions;
using CurrencyDomain = Main.Entities.Currency.Currency;

namespace Main.Tests.Domain.Currency;

public class CurrencyTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var currency = CurrencyDomain.Create("US Dollar", "USD", "$", "USD");

        currency.Name.Should().Be("US Dollar");
        currency.ShortName.Should().Be("USD");
        currency.CurrencySign.Should().Be("$");
        currency.Code.Should().Be("USD");
        currency.History.Should().BeEmpty();
    }

    [Fact]
    public void SetCurrencyToUsd_FirstTime_CreatesEntityAndHistory()
    {
        var currency = CurrencyDomain.Create("US Dollar", "USD", "$", "USD");

        currency.SetCurrencyToUsd(1.5m);

        currency.CurrencyToUsd.Should().NotBeNull();
        currency.CurrencyToUsd!.ToUsd.Should().Be(1.5m);

        currency.History.Should().HaveCount(1);

        var history = currency.History.First();
        history.PrevValue.Should().Be(0);
        history.NewValue.Should().Be(1.5m);
    }

    [Fact]
    public void SetCurrencyToUsd_SecondTime_UpdatesAndAddsHistory()
    {
        var currency = CurrencyDomain.Create("US Dollar", "USD", "$", "USD");

        currency.SetCurrencyToUsd(1.5m);
        currency.SetCurrencyToUsd(2.0m);

        currency.CurrencyToUsd!.ToUsd.Should().Be(2.0m);

        currency.History.Should().HaveCount(2);

        var last = currency.History.Last();
        last.PrevValue.Should().Be(1.5m);
        last.NewValue.Should().Be(2.0m);
    }

    [Fact]
    public void SetCurrencyToUsd_PersistsPreviousFromHistory()
    {
        var currency = CurrencyDomain.Create("US Dollar", "USD", "$", "USD");

        currency.SetCurrencyToUsd(10m);
        currency.SetCurrencyToUsd(15m);

        currency.History.Select(x => x.NewValue)
            .Should()
            .BeEquivalentTo([10m, 15m]);
    }
}