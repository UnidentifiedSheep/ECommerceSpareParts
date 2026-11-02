using Application.Common.Interfaces.Currency;
using FluentAssertions;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Currency;

namespace Tests.ServicesTests;

public class HistoricalCurrencyConverterTests : IntegrationTest
{
	public HistoricalCurrencyConverterTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<CurrencyRatesTestContext>();
	}

	[Fact]
	public async Task Convert_UsesNearestRateForEachRequestedTime()
	{
		var rate = GetContext<CurrencyRatesTestContext>().Rates.First();
		await SetHistory(rate, (new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), 0m, 2m),
			(new DateTime(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc), 2m, 4m));
		var converter = Scope.ServiceProvider.GetRequiredService<ICurrencyConverter>();
		var atFirst = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		(await converter.ConvertToBaseAsync(100m, rate.FromCurrencyId, atFirst, CancellationToken)).Should().Be(50m);
		(await converter.ConvertToBaseAsync(100m, rate.FromCurrencyId, atFirst.AddDays(30), CancellationToken)).Should().Be(25m);
		(await converter.ConvertAsync(100m, rate.FromCurrencyId, rate.ToCurrencyId, atFirst.AddMonths(1), CancellationToken)).Should().Be(25m);
		(await converter.ConvertFromBaseAsync(25m, rate.FromCurrencyId, atFirst.AddMonths(1), CancellationToken)).Should().Be(100m);
	}

	[Fact]
	public async Task Convert_BeforeFirstChange_UsesFirstKnownRate()
	{
		var rate = GetContext<CurrencyRatesTestContext>().Rates.First();
		var date = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		await SetHistory(rate, (date, 2m, 4m));
		(await Scope.ServiceProvider.GetRequiredService<ICurrencyConverter>()
			.ConvertToBaseAsync(100m, rate.FromCurrencyId, date.AddDays(-1), CancellationToken)).Should().Be(25m);
	}

	[Fact]
	public async Task Convert_EmptyHistory_ThrowsInsteadOfUsingCurrentRate()
	{
		var rate = GetContext<CurrencyRatesTestContext>().Rates.First();
		await SetHistory(rate);
		await Assert.ThrowsAsync<CurrencyRateNotFoundException>(() => Scope.ServiceProvider.GetRequiredService<ICurrencyConverter>()
			.ConvertToBaseAsync(100m, rate.FromCurrencyId, new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), CancellationToken));
	}

	private Task SetHistory(CurrencyRate rate, params (DateTime Date, decimal Prev, decimal Next)[] rows) =>
		GetContext<CurrencyRatesTestContext>().SetHistoryAsync(rate, rows, CancellationToken);
}
