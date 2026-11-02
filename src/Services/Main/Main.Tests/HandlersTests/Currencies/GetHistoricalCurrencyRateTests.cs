using FluentAssertions;
using Main.Application.Handlers.Currencies.GetCurrencyRate;
using Main.Entities.Exceptions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Currency;

namespace Tests.HandlersTests.Currencies;

public class GetHistoricalCurrencyRateTests : IntegrationTest
{
	public GetHistoricalCurrencyRateTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<CurrencyRatesTestContext>();
	}

	[Fact]
	public async Task Query_ReturnsRateAtRequestedInstantRegardlessOfOffset()
	{
		var rate = GetContext<CurrencyRatesTestContext>().Rates.First();
		var date = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		await GetContext<CurrencyRatesTestContext>().SetHistoryAsync(rate,
			[(date, 0m, 2m), (date.AddDays(10), 2m, 4m)], CancellationToken);
		var forDate = new DateTimeOffset(date.AddDays(1)).ToOffset(TimeSpan.FromHours(3));

		var result = await Mediator.Send(new GetHistoricalCurrencyRateQuery(rate.FromCurrencyId, forDate), CancellationToken);

		result.Rate.Should().Be(2m);
	}

	[Fact]
	public async Task Query_BaseCurrency_ReturnsOne()
	{
		var id = GetContext<CurrencyTestContext>().Currencies[0].Id;
		var result = await Mediator.Send(new GetHistoricalCurrencyRateQuery(id,
			new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)), CancellationToken);
		result.Rate.Should().Be(1m);
	}

	[Fact]
	public async Task Query_MissingHistory_ThrowsNotFound()
	{
		var rate = GetContext<CurrencyRatesTestContext>().Rates.First();
		await GetContext<CurrencyRatesTestContext>().SetHistoryAsync(rate, [], CancellationToken);
		var id = rate.FromCurrencyId;
		await Assert.ThrowsAsync<CurrencyRateNotFoundException>(() => Mediator.Send(new GetHistoricalCurrencyRateQuery(id,
			new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero)), CancellationToken));
	}
}
