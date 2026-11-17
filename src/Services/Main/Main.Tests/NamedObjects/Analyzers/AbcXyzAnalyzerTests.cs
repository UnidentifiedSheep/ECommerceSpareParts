using FluentAssertions;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Abstractions.Interfaces;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Analyzers.AbcXyz;
using Main.Modules.Analytics.Application.Models;
using Main.Modules.Analytics.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.Sale;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Sale;
using SaleContent = Main.Entities.Sale.Sale.SaleContent;

namespace Tests.NamedObjects.Analyzers;

public sealed class AbcXyzAnalyzerTests : IntegrationTest
{
	private static readonly DateTime WindowStart = new(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc);

	private static readonly DateTime WindowEnd = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

	private readonly List<Sample> _samples = [];

	private bool _cleaned;

	public AbcXyzAnalyzerTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<SaleTestContext>();

	private int ProductA => GetContext<ProductTestContext>().Products[0].Id;

	private int ProductB => GetContext<ProductTestContext>().Products[1].Id;

	private int ProductC => GetContext<ProductTestContext>().Products[2].Id;

	[Fact]
	public async Task Analyze_TakesProductFromSaleContentAndGroupsByProductAndMonth()
	{
		AddSale(ProductA, 0, 2, 100);
		AddSale(ProductA, 0, 3, 150);
		AddSale(ProductA, 1, 1, 50);
		AddSale(ProductB, 0, 4, 40);

		var result = await Analyze(Window());

		result.Items.Should().HaveCount(2);
		var a = result.Items.Single(item => item.ProductId == ProductA);
		a.Quantity.Should().Be(6m);
		a.Revenue.Should().Be(300m);
		a.PeriodsCount.Should().Be(12);
		var b = result.Items.Single(item => item.ProductId == ProductB);
		b.Quantity.Should().Be(4m);
		b.Revenue.Should().Be(40m);
		a.Rank.Should().Be(1);
		a.AbcClass.Should().Be(AbcClass.A);
	}

	[Fact]
	public async Task Analyze_ReadsPeriodsAsGroupedRowsInsteadOfSingleSales()
	{
		for (var i = 0; i < 5; i++)
			AddSale(ProductA, 3, 1, 10);

		var rows = await Read(Window());

		rows.Should().ContainSingle();
		rows[0].PeriodStart.Should().Be(WindowStart.AddMonths(3));
		rows[0].Quantity.Should().Be(5m);
		rows[0].Revenue.Should().Be(50m);
	}

	[Fact]
	public async Task Analyze_ExcludesDeactivatedAndOutOfWindowSales()
	{
		AddSale(ProductA, 0, 1, 100);
		AddSale(ProductA, 1, 1, 999, deactivate: true);
		AddSale(ProductA, -1, 1, 999);
		AddSale(ProductA, 12, 1, 999);

		var result = await Analyze(Window());

		result.Items.Single().Revenue.Should().Be(100m);
	}

	[Fact]
	public async Task Analyze_FiltersByOrganizationAndBuyer()
	{
		var buyer = GetContext<SaleTestContext>().Buyer.Id;
		AddSale(ProductA, 0, 1, 100);
		AddSale(ProductB, 0, 1, 100, organizationId: Guid.NewGuid(), buyerId: Guid.NewGuid());

		var byOrganization = await Analyze(Window() with { OrganizationId = buyer });
		var byBuyer = await Analyze(Window() with { BuyerId = buyer });
		var unknown = await Analyze(Window() with { BuyerId = Guid.NewGuid() });

		byOrganization.Items.Select(item => item.ProductId).Should().Equal(ProductA);
		byBuyer.Items.Select(item => item.ProductId).Should().Equal(ProductA);
		unknown.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task Analyze_RestrictsToSelectedSeasonMonths()
	{
		AddSale(ProductA, 0, 1, 10);
		AddSale(ProductA, 1, 1, 20);
		AddSale(ProductA, 2, 1, 30);

		var result = await Analyze(Window() with { Months = [(Month)WindowStart.AddMonths(1).Month] });

		result.Items.Single().Revenue.Should().Be(20m);
		result.Summary.PeriodsCount.Should().Be(1);
	}

	[Fact]
	public async Task Analyze_ClassifiesStableAndSporadicDemand()
	{
		for (var month = 0; month < 12; month++)
			AddSale(ProductA, month, 10, 100);
		AddSale(ProductB, 0, 1, 10);
		AddSale(ProductB, 7, 120, 20);
		for (var month = 0; month < 12; month += 2)
			AddSale(ProductC, month, 5, 1);

		var result = await Analyze(Window());

		var items = result.Items.ToDictionary(item => item.ProductId);
		items[ProductA].XyzClass.Should().Be(XyzClass.X);
		items[ProductB].XyzClass.Should().Be(XyzClass.Z);
		items[ProductC].XyzClass.Should().Be(XyzClass.Z);
		result.Summary.ProductsCount.Should().Be(3);
	}

	[Fact]
	public async Task Query_JoinsSaleContentsAndGroupsInDatabase()
	{
		var reader = Scope.ServiceProvider.GetRequiredService<AbcXyzDataReader>();

		var sql = reader.BuildQuery(Window()).ToQueryString();

		sql.Should().Contain("JOIN").And.Contain("GROUP BY");
	}

	private static AbcXyzParameters Window()
		=> new()
		{
			From = WindowStart,
			To = WindowEnd,
			AbcMetric = AnalysisMetric.Revenue,
			XyzMetric = AnalysisMetric.Quantity,
			AShare = 0.80m,
			BShare = 0.15m,
			XCoefficient = 0.10m,
			YCoefficient = 0.25m,
			MinPeriods = 6
		};

	private void AddSale(
		int productId,
		int monthIndex,
		int quantity,
		decimal revenue,
		bool deactivate = false,
		Guid? organizationId = null,
		Guid? buyerId = null)
		=> AddSale(
			productId,
			WindowStart.AddMonths(monthIndex).AddDays(3),
			quantity,
			revenue,
			deactivate,
			organizationId,
			buyerId);

	private void AddSale(
		int productId,
		DateTime date,
		int quantity,
		decimal revenue,
		bool deactivate = false,
		Guid? organizationId = null,
		Guid? buyerId = null)
		=> _samples.Add(new Sample(productId, date, quantity, revenue, deactivate, organizationId, buyerId));

	private async Task<AbcXyzAnalysisResult> Analyze(AbcXyzParameters parameters)
	{
		await Flush();
		var analyzer = Scope.ServiceProvider.GetRequiredService<IAbcXyzAnalyzer>();
		return await analyzer.AnalyzeAsync(parameters, CancellationToken);
	}

	private async Task<IReadOnlyList<ProductPeriodValue>> Read(
		AbcXyzParameters parameters)
	{
		await Flush();
		var reader = Scope.ServiceProvider.GetRequiredService<AbcXyzDataReader>();
		return await reader.ReadAsync(parameters, CancellationToken);
	}

	private async Task Flush()
	{
		var context = GetContext<SaleTestContext>();
		if (!_cleaned)
		{
			context.Sale.Delete();
			await Context.SaveChangesAsync(CancellationToken);
			DiscardPendingDomainEvents();
			await Context.Set<SaleContentAnalytics>().ExecuteDeleteAsync(CancellationToken);
			_cleaned = true;
		}

		if (_samples.Count == 0)
			return;

		var contents = new List<(SaleContent Content, Sample Sample)>();
		foreach (var sample in _samples)
		{
			var detail = new SaleContentDetailBuilder(Faker).WithCount(1)
				.WithCurrencyId(context.Sale.CurrencyId)
				.WithStorageContentIds([context.StorageContent.Id])
				.Build();
			var content = new SaleContentBuilder(Faker).WithProductId(sample.ProductId)
				.WithPrices(sample.Revenue, sample.Revenue)
				.WithDetails([detail])
				.Build();
			var sale = new SaleBuilder(Faker).WithUserId(context.Buyer.Id)
				.WithOrganizationId(context.Buyer.Id)
				.WithCurrencyId(context.Sale.CurrencyId)
				.WithStorageCode(context.Sale.StorageCode)
				.WithTransactionId(context.Transaction.Id)
				.WithSaleDate(sample.Date)
				.WithContents([content])
				.Completed()
				.Build();
			await Context.AddAsync(sale, CancellationToken);
			contents.Add((content, sample));
		}

		await Context.SaveChangesAsync(CancellationToken);
		DiscardPendingDomainEvents();

		foreach (var (content, sample) in contents)
		{
			var row = SaleContentAnalytics.Create(
				content.Id,
				content.SaleId,
				sample.OrganizationId ?? context.Buyer.Id,
				sample.BuyerId ?? context.Buyer.Id,
				sample.Date,
				sample.Quantity,
				sample.Revenue,
				sample.Revenue,
				0m);
			if (sample.Deactivate)
				row.Deactivate();
			await Context.AddAsync(row, CancellationToken);
		}

		_samples.Clear();
		await Context.SaveChangesAsync(CancellationToken);
		DiscardPendingDomainEvents();
	}

	private sealed record Sample(
		int ProductId,
		DateTime Date,
		int Quantity,
		decimal Revenue,
		bool Deactivate,
		Guid? OrganizationId,
		Guid? BuyerId);
}
