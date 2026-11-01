using FluentAssertions;
using Main.Application.Analytics.Interfaces;
using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.Sale;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Currency;

namespace Tests.ServicesTests;

public class SaleContentAnalyticsCalculatorTests : IntegrationTest
{
	public SaleContentAnalyticsCalculatorTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<CurrencyRatesTestContext>();
	}

	[Fact]
	public async Task Calculate_UsesSaleDateForRevenueAndIndividualPurchaseDatesForCost()
	{
		var rate = GetContext<CurrencyRatesTestContext>().Rates.First();
		var january = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var march = january.AddMonths(2);
		await Context.CurrencyRateHistories
			.Where(item => item.FromCurrencyId == rate.FromCurrencyId && item.ToCurrencyId == rate.ToCurrencyId)
			.ExecuteDeleteAsync(CancellationToken);
		foreach (var (date, previous, current) in new[] { (january, 0m, 2m), (march, 2m, 4m) })
		{
			var history = CurrencyRateHistory.Create(rate.FromCurrencyId, rate.ToCurrencyId, previous, current);
			await Context.AddAsync(history, CancellationToken);
			Context.Entry(history).Property(item => item.CreatedAt).CurrentValue = date;
		}
		await Context.SaveChangesAsync(CancellationToken);
		var content = new SaleContentBuilder(Faker).WithPrices(120m, 90m).WithDetails([
			new SaleContentDetailBuilder(Faker).WithCurrencyId(rate.FromCurrencyId).WithPurchaseDate(january)
				.WithPurchasePrice(40m).WithCount(2).Build(),
			new SaleContentDetailBuilder(Faker).WithCurrencyId(rate.FromCurrencyId).WithPurchaseDate(march)
				.WithPurchasePrice(40m).WithCount(1).Build(),
			new SaleContentDetailBuilder(Faker).WithCurrencyId(rate.ToCurrencyId).WithPurchaseDate(january)
				.WithPurchasePrice(7m).WithCount(1).Build()
		]).Build();

		var result = await Scope.ServiceProvider.GetRequiredService<ISaleContentAnalyticsCalculator>()
			.CalculateAsync(content, rate.FromCurrencyId, march, CancellationToken);

		result.OriginalAmount.Should().Be(120m);
		result.Revenue.Should().Be(90m);
		result.Cost.Should().Be(57m);
		content.Analytics.Should().BeNull();
	}
}
