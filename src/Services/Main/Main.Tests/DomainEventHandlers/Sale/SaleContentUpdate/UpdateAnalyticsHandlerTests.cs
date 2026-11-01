using Application.Common.Interfaces.Persistence;
using Attributes;
using FluentAssertions;
using Main.Entities.Exceptions;
using Main.Application.Analytics.Interfaces;
using Main.Entities.Sale;
using Tests.DataBuilders.Sale;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Sale;

namespace Tests.DomainEventHandlers.Sale.SaleContentUpdate;

public class UpdateAnalyticsHandlerTests : IntegrationTest
{
	public UpdateAnalyticsHandlerTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	[Fact]
	public async Task FinancialChange_UpdatesAnalyticsUsingHistoricalRate()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		var rate = GetContext<CurrencyRatesTestContext>().Rates.First();
		rate.SetRate(2m);
		await Context.SaveChangesAsync(CancellationToken);
		var date = DateTime.UtcNow;
		rate.SetRate(4m);
		await Context.SaveChangesAsync(CancellationToken);
		await Execute(async () =>
		{
			sale.SetCurrency(rate.FromCurrencyId);
			sale.SetDateTime(date);
			var content = sale.Contents.Single();
			content.SetPriceAndDetails(120m, 90m, content.Details.ToArray());
			await Context.SaveChangesAsync(CancellationToken);
		});
		var fact = await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(item => item.SaleContentId == sale.Contents.Single().Id, CancellationToken);
		fact.OriginalAmountInBaseCurrency.Should().Be(60m * sale.Contents.Single().Count);
		fact.RevenueInBaseCurrency.Should().Be(45m * sale.Contents.Single().Count);
		(await Context.SaleContentAnalytics.CountAsync(CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task CommentChange_PreservesAnalytics()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		var before = await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken);
		await Execute(async () => { sale.SetComment("changed"); await Context.SaveChangesAsync(CancellationToken); });
		(await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken)).Should().BeEquivalentTo(before);
	}

	[Fact]
	public async Task SoftDelete_PreservesAnalytics()
	{
		var before = await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken);
		await Execute(async () => { GetContext<SaleTestContext>().Sale.Delete(); await Context.SaveChangesAsync(CancellationToken); });
		(await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken)).Should().BeEquivalentTo(before);
	}

	[Fact]
	public async Task MissingHistoricalRate_RollsBackSaleAndAnalytics()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		var originalCurrency = sale.CurrencyId;
		var before = await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken);
		await Assert.ThrowsAsync<CurrencyRateNotFoundException>(() => Execute(async () =>
		{
			sale.SetCurrency(GetContext<CurrencyRatesTestContext>().Rates.First().FromCurrencyId);
			sale.SetDateTime(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			await Context.SaveChangesAsync(CancellationToken);
		}));
		(await Context.Sales.AsNoTracking().SingleAsync(item => item.Id == sale.Id, CancellationToken)).CurrencyId.Should().Be(originalCurrency);
		(await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken)).Should().BeEquivalentTo(before);
	}

	[Fact]
	public async Task FinancialChange_UpdatesOnlyAffectedContentAndPreservesOtherSales()
	{
		var context = GetContext<SaleTestContext>();
		var second = NewContent();
		context.Sale.AddContent(second);
		var other = new SaleBuilder(Faker).WithUserId(context.Buyer.Id).WithOrganizationId(context.Buyer.Id)
			.WithCurrencyId(context.Sale.CurrencyId).WithStorageCode(context.Sale.StorageCode)
			.WithTransactionId(context.Transaction.Id).WithContents([NewContent()]).Completed().Build();
		await Context.AddAsync(other, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var secondFact = SaleContentAnalytics.Create(second.Id, 1m, 1m, 1m);
		var otherFact = SaleContentAnalytics.Create(other.Contents.Single().Id, 2m, 2m, 2m);
		await Context.AddRangeAsync([secondFact, otherFact], CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var content = context.Sale.Contents.First();
		var originalFact = content.Analytics;
		await Execute(async () =>
		{
			content.SetPriceAndDetails(150m, 100m, content.Details.ToArray());
			await Context.SaveChangesAsync(CancellationToken);
		});
		// Repeated changes update the same tracked analytics instance.
		await Execute(async () =>
		{
			content.SetPriceAndDetails(160m, 110m, content.Details.ToArray());
			await Context.SaveChangesAsync(CancellationToken);
		});

		var facts = await Context.SaleContentAnalytics.AsNoTracking().ToDictionaryAsync(item => item.SaleContentId, CancellationToken);
		facts.Should().HaveCount(3);
		facts[content.Id].RevenueInBaseCurrency.Should().Be(110m * content.Count);
		facts[second.Id].RevenueInBaseCurrency.Should().Be(1m);
		content.Analytics.Should().BeSameAs(originalFact);
		second.Analytics.Should().BeSameAs(secondFact);
		facts[otherFact.SaleContentId].RevenueInBaseCurrency.Should().Be(2m);
	}

	[Fact]
	public async Task ContentCommentChange_DoesNotRebuildAnalytics()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		var fact = await Context.SaleContentAnalytics.SingleAsync(CancellationToken);
		fact.Update(1m, 1m, 1m);
		await Context.SaveChangesAsync(CancellationToken);

		await Execute(async () =>
		{
			sale.Contents.Single().SetComment("comment only");
			await Context.SaveChangesAsync(CancellationToken);
		});

		(await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken)).RevenueInBaseCurrency.Should().Be(1m);
	}

	[Fact]
	public async Task AddingContent_CreatesAnalyticsForNewContent()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		var content = NewContent();
		await Execute(async () =>
		{
			sale.AddContent(content);
			await Context.SaveChangesAsync(CancellationToken);
		});

		var facts = await Context.SaleContentAnalytics.AsNoTracking().ToDictionaryAsync(item => item.SaleContentId, CancellationToken);
		facts.Keys.Should().BeEquivalentTo(sale.Contents.Select(item => item.Id));
		facts[content.Id].RevenueInBaseCurrency.Should().Be(content.TotalSum);
	}

	[Fact]
	public async Task RemovingContent_CascadesAnalyticsDeletionAndPreservesRemainingContents()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		var remaining = NewContent();
		sale.AddContent(remaining);
		await Context.SaveChangesAsync(CancellationToken);
		await Context.AddAsync(SaleContentAnalytics.Create(remaining.Id, 1m, 1m, 1m), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var removed = sale.Contents.First();

		await Execute(async () =>
		{
			sale.RemoveContent(removed);
			Context.Remove(removed);
			await Context.SaveChangesAsync(CancellationToken);
		});

		var fact = await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken);
		fact.SaleContentId.Should().Be(remaining.Id);
		fact.RevenueInBaseCurrency.Should().Be(1m);
	}

	[Fact]
	public async Task FailureAfterUpdate_RollsBackAnalyticsChanges()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		var before = await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken);
		await Assert.ThrowsAsync<InvalidOperationException>(() => Execute(async () =>
		{
			var content = sale.Contents.Single();
			content.SetPriceAndDetails(150m, 100m, content.Details.ToArray());
			await Context.SaveChangesAsync(CancellationToken);
			await Scope.ServiceProvider.GetRequiredService<ISaleAnalyticsSynchronizer>().SynchronizeAsync(sale.Contents, CancellationToken);
			await Context.SaveChangesAsync(CancellationToken);
			throw new InvalidOperationException("Abort after updating analytics.");
		}));

		(await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken)).Should().BeEquivalentTo(before);
	}

	[Fact]
	public async Task SaleDateChange_UpdatesAnalyticsForAllContents()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		var second = NewContent();
		sale.AddContent(second);
		await Context.SaveChangesAsync(CancellationToken);
		await Context.AddAsync(SaleContentAnalytics.Create(second.Id, 1m, 1m, 1m), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		await Execute(async () =>
		{
			sale.SetDateTime(sale.SaleDatetime.AddDays(-1));
			await Context.SaveChangesAsync(CancellationToken);
		});

		var facts = await Context.SaleContentAnalytics.AsNoTracking().ToDictionaryAsync(item => item.SaleContentId, CancellationToken);
		foreach (var content in sale.Contents)
			facts[content.Id].RevenueInBaseCurrency.Should().Be(content.TotalSum);
	}

	[Fact]
	public async Task FinancialChangeFollowedByContentRemoval_DoesNotRecreateDeletedAnalytics()
	{
		var sale = GetContext<SaleTestContext>().Sale;
		var removed = sale.Contents.Single();
		var remaining = NewContent();
		sale.AddContent(remaining);
		await Context.SaveChangesAsync(CancellationToken);
		await Context.AddAsync(SaleContentAnalytics.Create(remaining.Id, 1m, 1m, 1m), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		await Execute(async () =>
		{
			sale.SetDateTime(sale.SaleDatetime.AddDays(-1));
			sale.RemoveContent(removed);
			Context.Remove(removed);
			await Context.SaveChangesAsync(CancellationToken);
		});

		var fact = await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(CancellationToken);
		fact.SaleContentId.Should().Be(remaining.Id);
		fact.RevenueInBaseCurrency.Should().Be(remaining.TotalSum);
	}

	private SaleContent NewContent()
	{
		var context = GetContext<SaleTestContext>();
		var detail = new SaleContentDetailBuilder(Faker).WithCount(1).WithPurchasePrice(40m)
			.WithCurrencyId(context.Sale.CurrencyId).WithStorageContentIds([context.StorageContent.Id]).Build();
		return new SaleContentBuilder(Faker).WithProductId(context.Product.Id).WithPrices(120m, 90m).WithDetails([detail]).Build();
	}

	private Task Execute(Func<Task> action) => Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>()
		.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (_, _) => await action(), CancellationToken);
}
