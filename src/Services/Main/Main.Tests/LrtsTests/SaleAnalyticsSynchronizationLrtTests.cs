using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Analytics.Lrts.SaleAnalyticsSynchronization;
using Main.Application.Lrts.Base.Synchronization;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tests.DataBuilders.Sale;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Sale;

namespace Tests.LrtsTests;

public class SaleAnalyticsSynchronizationLrtTests : IntegrationTest
{
	public SaleAnalyticsSynchronizationLrtTests(CombinedContainerFixture fixture) : base(fixture) => RegisterBasicContext<SaleTestContext>();

	[Fact]
	public async Task Synchronize_RebuildsMissingAndExistingFactsWithoutDuplicates()
	{
		await AddSales();
		var context = GetContext<SaleTestContext>();
		for (var index = 0; index < 2; index++)
		{
			var detail = new SaleContentDetailBuilder(Faker).WithCount(1).WithPurchasePrice(40m)
				.WithCurrencyId(context.Sale.CurrencyId).WithStorageContentIds([context.StorageContent.Id]).Build();
			context.Sale.AddContent(new SaleContentBuilder(Faker).WithProductId(context.Product.Id)
				.WithPrices(120m, 90m).WithDetails([detail]).Build());
		}
		await Context.SaveChangesAsync(CancellationToken);
		await Context.SaleContentAnalytics.ExecuteDeleteAsync(CancellationToken);
		var first = await Execute();
		first.Status.Should().Be(JobStatus.Succeeded, first.ErrorMessage);
		var (_, checkpoint) = LrtStateSerializer.Deserialize<NoneInputState, SynchronizationState<Guid>>(first.State);
		checkpoint!.ProcessedCount.Should().Be(4);
		var before = await Context.SaleContentAnalytics.AsNoTracking().OrderBy(item => item.SaleContentId).ToListAsync(CancellationToken);
		before.Should().HaveCount(6);
		var second = await Execute();
		second.Status.Should().Be(JobStatus.Succeeded, second.ErrorMessage);
		(await Context.SaleContentAnalytics.AsNoTracking().OrderBy(item => item.SaleContentId).ToListAsync(CancellationToken))
			.Should().BeEquivalentTo(before);
	}

	[Fact]
	public async Task Failure_PreservesCompletedBatchAndResumeSkipsItsRows()
	{
		var bad = await AddSales(invalidLast: true);
		await Context.SaleContentAnalytics.ExecuteDeleteAsync(CancellationToken);
		var failed = await Execute();
		failed.Status.Should().NotBe(JobStatus.Succeeded);
		var (_, checkpoint) = LrtStateSerializer.Deserialize<NoneInputState, SynchronizationState<Guid>>(failed.State);
		checkpoint!.ProcessedCount.Should().Be(2);
		(await Context.SaleContentAnalytics.CountAsync(CancellationToken)).Should().Be(2);

		var detail = new SaleContentDetailBuilder(Faker).WithCount(1).WithCurrencyId(GetContext<SaleTestContext>().Sale.CurrencyId)
			.WithStorageContentIds([GetContext<SaleTestContext>().StorageContent.Id]).WithPurchasePrice(40m).Build();
		bad.SetPriceAndDetails(120m, 90m, [detail]);
		await Context.SaveChangesAsync(CancellationToken);
		var retained = await Context.SaleContentAnalytics.OrderBy(item => item.SaleContentId).FirstAsync(CancellationToken);
		retained.Update(1m, 1m, 1m);
		await Context.SaveChangesAsync(CancellationToken);

		var resumed = await Execute(checkpoint);
		resumed.Status.Should().Be(JobStatus.Succeeded, resumed.ErrorMessage);
		(await Context.SaleContentAnalytics.CountAsync(CancellationToken)).Should().Be(4);
		(await Context.SaleContentAnalytics.AsNoTracking().SingleAsync(item => item.SaleContentId == retained.SaleContentId, CancellationToken))
			.RevenueInBaseCurrency.Should().Be(1m);
	}

	[Fact]
	public async Task Synchronize_SoftDeletedSale_RemovesItsFacts()
	{
		GetContext<SaleTestContext>().Sale.Delete();
		await Context.SaveChangesAsync(CancellationToken);
		var job = await Execute();
		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		(await Context.SaleContentAnalytics.AnyAsync(CancellationToken)).Should().BeFalse();
	}

	private async Task<SaleContent> AddSales(bool invalidLast = false)
	{
		var context = GetContext<SaleTestContext>();
		var sales = new List<Main.Entities.Sale.Sale> { context.Sale };
		for (var index = 0; index < 3; index++)
		{
			var detail = new SaleContentDetailBuilder(Faker).WithCount(1).WithPurchasePrice(40m)
				.WithStorageContentIds([context.StorageContent.Id]).WithCurrencyId(context.Sale.CurrencyId).Build();
			var content = new SaleContentBuilder(Faker).WithProductId(context.Product.Id).WithPrices(120m, 90m).WithDetails([detail]).Build();
			var sale = new SaleBuilder(Faker).WithUserId(context.Buyer.Id).WithOrganizationId(context.Buyer.Id)
				.WithCurrencyId(context.Sale.CurrencyId).WithStorageCode(context.Sale.StorageCode)
				.WithTransactionId(context.Transaction.Id).WithContents([content]).Completed().Build();
			sales.Add(sale);
			await Context.AddAsync(sale, CancellationToken);
		}

		var last = sales.OrderBy(sale => sale.Id).Last().Contents.Single();
		if (invalidLast)
		{
			var detail = new SaleContentDetailBuilder(Faker).WithCount(1).WithPurchasePrice(40m)
				.WithStorageContentIds([context.StorageContent.Id])
				.WithCurrencyId(GetContext<CurrencyRatesTestContext>().Rates.First().FromCurrencyId)
				.WithPurchaseDate(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Build();
			last.SetPriceAndDetails(120m, 90m, [detail]);
		}
		await Context.SaveChangesAsync(CancellationToken);
		return last;
	}

	private async Task<SingleRunJob> Execute(SynchronizationState<Guid>? checkpoint = null)
	{
		Scope.ServiceProvider.GetRequiredService<PublishEndpointAccessor>().Publisher = new MessageBrokerStub();
		var lrt = ActivatorUtilities.CreateInstance<SmallBatchLrt>(Scope.ServiceProvider);
		var state = checkpoint is null ? lrt.ValidateState("{}") : LrtStateSerializer.Serialize(new NoneInputState(), checkpoint);
		var job = SingleRunJob.Create(lrt.SystemName, state, 3);
		var holder = Guid.NewGuid();
		job.AcquireLease(holder, TimeSpan.FromMinutes(5));
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		await lrt.ExecuteAsync(job.Id, holder, CancellationToken);
		await Context.Entry(job).ReloadAsync(CancellationToken);
		return job;
	}

	private sealed class SmallBatchLrt(IApplicationTransactionFactory transactions, ILogger<SaleAnalyticsSynchronizationLrt> logger)
		: SaleAnalyticsSynchronizationLrt(transactions, logger)
	{
		protected override int BatchSize => 2;
	}
}
