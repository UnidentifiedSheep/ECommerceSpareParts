using Enums;
using FluentAssertions;
using Main.Entities.Logistics.Enums;
using Main.Modules.Logistics.Abstractions;
using Main.Modules.Logistics.Abstractions.Interfaces;
using Main.Modules.Logistics.Abstractions.Models;
using Main.Modules.Logistics.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.Logistics;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.ServicesTests.Logistics;

public sealed class DeliveryEstimatorTests : IntegrationTest
{
	private static readonly DateOnly Monday = new(2026, 11, 23);

	public DeliveryEstimatorTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<StorageTestContext>();

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	[Fact]
	public async Task Estimate_WithSchedule_ShiftsToReceivingDayAndCountsReceivingDays()
	{
		await new DeliveryScheduleBuilder(Faker)
			.WithSupplier(Supplier.Armtek)
			.WithStorageCode(StorageCode)
			.WithDays(DeliveryDays.Tuesday | DeliveryDays.Thursday)
			.BuildAndAddToDb(Context, CancellationToken);
		var friday = Monday.AddDays(4);

		var result = await (await GetEstimatorAsync(Supplier.Armtek)).EstimateAsync(
			Supplier.Armtek,
			StorageCode,
			friday,
			[friday.AddDays(3), friday.AddDays(4), friday.AddDays(-1)],
			CancellationToken);

		result[friday.AddDays(3)].Should().Be(new DeliveryEstimate(friday.AddDays(4), 1));
		result[friday.AddDays(4)].Should().Be(new DeliveryEstimate(friday.AddDays(4), 1));
		result[friday.AddDays(-1)].Should().Be(new DeliveryEstimate(friday.AddDays(4), 1));
	}

	[Fact]
	public async Task Estimate_SkipsExceptionDays()
	{
		await new DeliveryScheduleBuilder(Faker)
			.WithSupplier(Supplier.Armtek)
			.WithStorageCode(StorageCode)
			.WithDays(DeliveryDays.Tuesday | DeliveryDays.Thursday)
			.WithException(Monday.AddDays(1))
			.BuildAndAddToDb(Context, CancellationToken);

		var result = await (await GetEstimatorAsync(Supplier.Armtek)).EstimateAsync(
			Supplier.Armtek,
			StorageCode,
			Monday,
			[Monday.AddDays(1)],
			CancellationToken);

		result[Monday.AddDays(1)].Should().Be(new DeliveryEstimate(Monday.AddDays(3), 1));
	}

	[Fact]
	public async Task Estimate_WithoutSchedule_UsesCalendarDays()
	{
		var result = await (await GetEstimatorAsync(Supplier.Tmtr)).EstimateAsync(
			Supplier.Tmtr,
			StorageCode,
			Monday,
			[Monday.AddDays(6), Monday.AddDays(-2)],
			CancellationToken);

		result[Monday.AddDays(6)].Should().Be(new DeliveryEstimate(Monday.AddDays(6), 6));
		result[Monday.AddDays(-2)].Should().Be(new DeliveryEstimate(Monday, 0));
	}

	private async Task<IDeliveryEstimator> GetEstimatorAsync(Supplier supplier)
	{
		await Scope.ServiceProvider.GetRequiredService<IDeliveryScheduleCacheRepository>()
			.InvalidateAsync(supplier, StorageCode, CancellationToken);
		return Scope.ServiceProvider.GetRequiredService<IDeliveryEstimator>();
	}
}
