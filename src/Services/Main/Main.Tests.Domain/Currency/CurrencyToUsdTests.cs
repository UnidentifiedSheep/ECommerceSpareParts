using FluentAssertions;
using Main.Entities.Currency;

namespace Main.Tests.Domain.Currency;

public class CurrencyToUsdTests
{
    [Fact]
    public void Create_Valid_Succeeds()
    {
        var entity = CurrencyToUsd.Create(1, 2.5m);

        entity.CurrencyId.Should().Be(1);
        entity.ToUsd.Should().Be(2.5m);
    }

    [Fact]
    public void SetToUsd_NegativeOrZero_Throws()
    {
        var entity = CurrencyToUsd.Create(1, 1m);

        var act1 = () => entity.SetToUsd(0m);
        var act2 = () => entity.SetToUsd(-1m);

        act1.Should().Throw<ArgumentOutOfRangeException>();
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetToUsd_Valid_UpdatesValue()
    {
        var entity = CurrencyToUsd.Create(1, 1m);

        entity.SetToUsd(3.14m);

        entity.ToUsd.Should().Be(3.14m);
    }
}