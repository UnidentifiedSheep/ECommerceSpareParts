using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Persistence;
using Attributes;
using FluentAssertions;
using Integrations.Common;
using Integrations.ExchangeRate.Interfaces;
using Integrations.ExchangeRate.Models;
using Main.Application.Models.Currency;
using Main.Application.Services.Currency;
using Main.Entities.Currency;
using Main.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Currency;

namespace Tests.ServicesTests;

public sealed class CurrencyRateUpdaterTests : IntegrationTest
{
	public CurrencyRateUpdaterTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<CurrencyRatesTestContext>();

	private CurrencyRatesTestContext Rates => GetContext<CurrencyRatesTestContext>();
	private Currency BaseCurrency => Rates.CurrencyTestContext.Currencies.Single(currency => currency.Id == Rates.Rates.First().ToCurrencyId);

	[Fact]
	public async Task InitialUpdate_CreatesRates_AndRepeatedUpdateDoesNotDuplicateThem()
	{
		await Context.CurrencyRates.ExecuteDeleteAsync(CancellationToken);
		var values = Quotes();

		var first = await Update(values);
		var second = await Update(values);

		first.Changed.Should().HaveCount(Rates.Rates.Count);
		second.Changed.Should().BeEmpty();
		(await Context.CurrencyRates.CountAsync(CancellationToken)).Should().Be(Rates.Rates.Count);
		var rows = await Context.CurrencyRates.AsNoTracking().ToListAsync(CancellationToken);
		rows.Should().OnlyContain(row => row.Id > 0 && row.ToCurrencyId == BaseCurrency.Id);
		foreach (var row in rows)
			row.Rate.Should().Be(values[Rates.CurrencyTestContext.Currencies.Single(currency => currency.Id == row.FromCurrencyId).Code]);
	}

	[Fact]
	public async Task ChangedRate_AppendsObservation_AndPreservesEarlierValue()
	{
		var values = Quotes();
		await Update(values);
		var previous = await Context.CurrencyRates.AsNoTracking().OrderByDescending(row => row.CreatedAt).ThenByDescending(row => row.Id).FirstAsync(CancellationToken);
		var currency = Rates.CurrencyTestContext.Currencies.Single(row => row.Id == previous.FromCurrencyId);
		var count = await Context.CurrencyRates.CountAsync(CancellationToken);
		values[currency.Code] = previous.Rate + 1m;

		var result = await Update(values);

		result.Changed.Should().ContainSingle().Which.Key.Should().Be(currency.Id);
		(await Context.CurrencyRates.CountAsync(CancellationToken)).Should().Be(count + 1);
		(await Context.CurrencyRates.AsNoTracking().SingleAsync(row => row.Id == previous.Id, CancellationToken)).Rate.Should().Be(previous.Rate);
	}

	[Fact]
	public async Task LatestObservation_IsUsedForComparisonRegardlessOfInsertionOrder()
	{
		var selected = Rates.Rates.First();
		var date = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		await Rates.SetHistoryAsync(selected, [(date.AddDays(1), 4m), (date, 2m)], CancellationToken);
		var values = Quotes();
		var currency = Rates.CurrencyTestContext.Currencies.Single(row => row.Id == selected.FromCurrencyId);
		values[currency.Code] = 4m;

		var result = await Update(values);

		result.Changed.Should().NotContainKey(currency.Id);
		(await Context.CurrencyRates.CountAsync(row => row.FromCurrencyId == currency.Id, CancellationToken)).Should().Be(2);
	}

	[Fact]
	public async Task MissingQuote_IsReportedWithoutDeletingPreviousRates()
	{
		var values = Quotes();
		await Update(values);
		var missing = Rates.CurrencyTestContext.Currencies.First(currency => currency.Id != BaseCurrency.Id);
		values.Remove(missing.Code);
		var count = await Context.CurrencyRates.CountAsync(CancellationToken);

		var result = await Update(values);

		result.NotFound.Should().Equal(missing.Code);
		result.Changed.Should().BeEmpty();
		(await Context.CurrencyRates.CountAsync(CancellationToken)).Should().Be(count);
	}

	[Fact]
	public async Task ConcurrentUpdates_WithSameQuotes_DoNotCreateDuplicateObservations()
	{
		await Context.CurrencyRates.ExecuteDeleteAsync(CancellationToken);
		var values = Quotes();

		var results = await Task.WhenAll(Update(values), Update(values));

		results.Sum(result => result.Changed.Count).Should().Be(Rates.Rates.Count);
		(await Context.CurrencyRates.CountAsync(CancellationToken)).Should().Be(Rates.Rates.Count);
	}

	private Dictionary<string, decimal> Quotes() => Rates.CurrencyTestContext.Currencies
		.Where(currency => currency.Id != BaseCurrency.Id).Select((currency, index) => (currency.Code, Rate: 2m + index))
		.ToDictionary(value => value.Code, value => value.Rate);

	private async Task<UpdateRatesResult> Update(Dictionary<string, decimal> quotes)
	{
		var client = new Mock<IExchangeRateClient>();
		client.Setup(value => value.GetRates(It.IsAny<CancellationToken>()))
			.ReturnsAsync(Response<ExchangeRates>.Ok(new ExchangeRates(BaseCurrency.Code, quotes)));
		var factory = new Mock<IExchangeRateClientFactory>();
		factory.Setup(value => value.GetClient(It.IsAny<Enums.ExchangeRateProvider>())).Returns(client.Object);
		await using var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionFactory>().Create();
		return await transaction.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (ctx, ct) =>
		{
			var updater = new CurrencyRateUpdater(factory.Object, ctx.Repositories.Get<Currency, int>(),
				ctx.Repositories.GetForRead<CurrencyRate, int>(), ctx.GetRequiredService<ICurrencyConverter>(), ctx.UnitOfWork);
			var result = await updater.UpdateAsync(new CurrencySetting(new CurrencySettingData { BaseCurrencyId = BaseCurrency.Id }), ct);
			await ctx.UnitOfWork.SaveChangesAsync(ct);
			return result;
		}, CancellationToken);
	}
}
