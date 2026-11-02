using FluentAssertions;
using Main.Entities.Currency;

namespace Tests.Domain.Currency;

public sealed class CurrencyRateTests
{
	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void NonPositiveRate_IsRejected(decimal value) =>
		Assert.Throws<InvalidOperationException>(() => CurrencyRate.Create(1, 2, value));

	[Fact]
	public void SameCurrencyPair_IsRejected() =>
		Assert.Throws<InvalidOperationException>(() => CurrencyRate.Create(1, 1, 2m));

	[Fact]
	public void Create_PreservesCurrencyPairAndRate()
	{
		var row = CurrencyRate.Create(1, 2, 3m);
		row.FromCurrencyId.Should().Be(1);
		row.ToCurrencyId.Should().Be(2);
		row.Rate.Should().Be(3m);
	}
}
