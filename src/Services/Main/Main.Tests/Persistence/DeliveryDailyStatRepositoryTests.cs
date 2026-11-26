using System.Reflection;
using Enums;
using FluentAssertions;
using Main.Entities.Logistics.Statistics;
using Main.Modules.Logistics.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Storage;

namespace Tests.Persistence;

public sealed class DeliveryDailyStatRepositoryTests : IntegrationTest
{
	private static readonly DateTime Morning = new(2026, 11, 25, 10, 0, 0, DateTimeKind.Utc);
	private static readonly DateTime Noon = new(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
	private static readonly DateTime Afternoon = new(2026, 11, 25, 15, 0, 0, DateTimeKind.Utc);

	public DeliveryDailyStatRepositoryTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	private IDeliveryDailyStatRepository Repository
		=> Scope.ServiceProvider.GetRequiredService<IDeliveryDailyStatRepository>();

	private int ProductId => GetContext<ProductTestContext>().Products[0].Id;

	private int OtherProductId => GetContext<ProductTestContext>().Products[1].Id;

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	[Fact]
	public async Task Upsert_NewRow_PersistsEveryProperty()
	{
		var stat = DeliveryDailyStat.Merge(
			Stat(Morning, deliveryDays: 3, probability: 80, guaranteedDays: 5, supplier: Supplier.Tmtr),
			Stat(Afternoon, deliveryDays: 3, probability: 90, guaranteedDays: 4, supplier: Supplier.Tmtr));

		var properties = typeof(DeliveryDailyStat)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		properties.Should().OnlyContain(
			property => !IsDefault(property.GetValue(stat)),
			"every property must be filled so that a column forgotten in UpsertAsync is detected");

		await Repository.UpsertAsync([stat], CancellationToken);

		var saved = (await GetStatsAsync()).Should().ContainSingle().Subject;
		saved.Should().BeEquivalentTo(stat);
	}

	[Fact]
	public async Task Upsert_ExistingRow_AccumulatesCountAndKeepsBestValues()
	{
		await Repository.UpsertAsync(
			[Stat(Noon, deliveryDays: 3, probability: 80, guaranteedDays: 5)],
			CancellationToken);

		await Repository.UpsertAsync(
			[
				DeliveryDailyStat.Merge(
					Stat(Morning, deliveryDays: 3, probability: 70, guaranteedDays: 6),
					Stat(Afternoon, deliveryDays: 3, probability: 90, guaranteedDays: 4))
			],
			CancellationToken);

		var saved = (await GetStatsAsync()).Should().ContainSingle().Subject;
		saved.OffersCount.Should().Be(3);
		saved.MaxProbability.Should().Be(90);
		saved.MinGuaranteedDays.Should().Be(4);
		saved.FirstSeenAt.Should().Be(Morning);
		saved.LastSeenAt.Should().Be(Afternoon);
	}

	[Fact]
	public async Task Upsert_ExistingRow_KeepsWorseValuesUntouched()
	{
		await Repository.UpsertAsync(
			[Stat(Noon, deliveryDays: 3, probability: 90, guaranteedDays: 4)],
			CancellationToken);

		await Repository.UpsertAsync(
			[Stat(Noon, deliveryDays: 3, probability: 50, guaranteedDays: 9)],
			CancellationToken);

		var saved = (await GetStatsAsync()).Should().ContainSingle().Subject;
		saved.OffersCount.Should().Be(2);
		saved.MaxProbability.Should().Be(90);
		saved.MinGuaranteedDays.Should().Be(4);
		saved.FirstSeenAt.Should().Be(Noon);
		saved.LastSeenAt.Should().Be(Noon);
	}

	[Theory]
	[InlineData(null, 4, 4)]
	[InlineData(4, null, 4)]
	[InlineData(null, null, null)]
	public async Task Upsert_GuaranteedDays_NullIsIgnored(int? existing, int? incoming, int? expected)
	{
		await Repository.UpsertAsync(
			[Stat(Morning, deliveryDays: 3, probability: 90, guaranteedDays: existing)],
			CancellationToken);

		await Repository.UpsertAsync(
			[Stat(Afternoon, deliveryDays: 3, probability: 90, guaranteedDays: incoming)],
			CancellationToken);

		(await GetStatsAsync()).Single().MinGuaranteedDays.Should().Be(expected);
	}

	[Fact]
	public async Task Upsert_DifferentKeyParts_StoredAsSeparateRows()
	{
		await Repository.UpsertAsync(
			[
				Stat(Morning, deliveryDays: 1, probability: 90, guaranteedDays: null),
				Stat(Morning, deliveryDays: 90, probability: 90, guaranteedDays: null),
				Stat(Morning.AddDays(1), deliveryDays: 1, probability: 90, guaranteedDays: null),
				Stat(Morning, deliveryDays: 1, probability: 90, guaranteedDays: null, supplier: Supplier.Tmtr),
				Stat(Morning, deliveryDays: 1, probability: 90, guaranteedDays: null, productId: OtherProductId)
			],
			CancellationToken);

		var stats = await GetStatsAsync();

		stats.Should().HaveCount(5);
		stats.Should().OnlyContain(x => x.OffersCount == 1);
	}

	[Fact]
	public async Task Upsert_Empty_DoesNothing()
	{
		await Repository.UpsertAsync([], CancellationToken);

		(await GetStatsAsync()).Should().BeEmpty();
	}

	private DeliveryDailyStat Stat(
		DateTime seenAt,
		int deliveryDays,
		int probability,
		int? guaranteedDays,
		Supplier supplier = Supplier.Armtek,
		int? productId = null)
		=> DeliveryDailyStat.Create(
			supplier,
			StorageCode,
			productId ?? ProductId,
			deliveryDays,
			probability,
			guaranteedDays,
			seenAt);

	private Task<List<DeliveryDailyStat>> GetStatsAsync()
		=> Context.Set<DeliveryDailyStat>().AsNoTracking().ToListAsync(CancellationToken);

	private static bool IsDefault(object? value)
		=> value is null || (value.GetType().IsValueType && value.Equals(Activator.CreateInstance(value.GetType())));
}
