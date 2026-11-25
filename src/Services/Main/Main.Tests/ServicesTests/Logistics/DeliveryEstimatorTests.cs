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
	public async Task Estimate_WithoutSchedule_UsesWorkingDays()
	{
		var result = await (await GetEstimatorAsync(Supplier.Tmtr)).EstimateAsync(
			Supplier.Tmtr,
			StorageCode,
			Monday,
			[Monday.AddDays(4), Monday.AddDays(6), Monday.AddDays(-2)],
			CancellationToken);

		result[Monday.AddDays(4)].Should().Be(new DeliveryEstimate(Monday.AddDays(4), 4));
		result[Monday.AddDays(6)].Should().Be(new DeliveryEstimate(Monday.AddDays(7), 5));
		result[Monday.AddDays(-2)].Should().Be(new DeliveryEstimate(Monday, 0));
	}

	[Fact]
	public async Task GetArrivalDates_WithSchedule_ReturnsNthReceivingDayAtArrivalTime()
	{
		await new DeliveryScheduleBuilder(Faker)
			.WithSupplier(Supplier.Armtek)
			.WithStorageCode(StorageCode)
			.WithDays(DeliveryDays.Tuesday | DeliveryDays.Thursday)
			.WithArrivalTime(new TimeOnly(14, 0))
			.BuildAndAddToDb(Context, CancellationToken);
		var friday = At(Monday.AddDays(4), 10);

		var result = await (await GetEstimatorAsync(Supplier.Armtek)).GetArrivalDatesAsync(
			Supplier.Armtek,
			StorageCode,
			friday,
			[0, 1, 2, 1],
			CancellationToken);

		result.Should().BeEquivalentTo(new Dictionary<int, DateTime>
		{
			[0] = At(Monday.AddDays(8), 14),
			[1] = At(Monday.AddDays(8), 14),
			[2] = At(Monday.AddDays(10), 14)
		});
	}

	[Theory]
	[InlineData(10, 0, 14)]
	[InlineData(16, 1, 14)]
	public async Task GetArrivalDates_SameDayAfterArrivalTime_MovesToNextReceivingDay(
		int hour,
		int expectedDayOffset,
		int expectedHour)
	{
		await new DeliveryScheduleBuilder(Faker)
			.WithSupplier(Supplier.Armtek)
			.WithStorageCode(StorageCode)
			.WithDays(DeliveryDays.All)
			.WithArrivalTime(new TimeOnly(14, 0))
			.BuildAndAddToDb(Context, CancellationToken);

		var result = await (await GetEstimatorAsync(Supplier.Armtek)).GetArrivalDatesAsync(
			Supplier.Armtek,
			StorageCode,
			At(Monday, hour),
			[0],
			CancellationToken);

		result[0].Should().Be(At(Monday.AddDays(expectedDayOffset), expectedHour));
	}

	[Fact]
	public async Task GetArrivalDates_WithoutSchedule_UsesWorkingDaysAtEndOfDay()
	{
		var result = await (await GetEstimatorAsync(Supplier.Tmtr)).GetArrivalDatesAsync(
			Supplier.Tmtr,
			StorageCode,
			At(Monday, 10),
			[0, 4, 6],
			CancellationToken);

		result[0].Should().Be(EndOfDay(Monday));
		result[4].Should().Be(EndOfDay(Monday.AddDays(4)));
		result[6].Should().Be(EndOfDay(Monday.AddDays(8)));
	}

	private static DateTime At(DateOnly date, int hour)
		=> date.ToDateTime(new TimeOnly(hour, 0), DateTimeKind.Utc);

	private static DateTime EndOfDay(DateOnly date)
		=> date.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc);

	private async Task<IDeliveryEstimator> GetEstimatorAsync(Supplier supplier)
	{
		await Scope.ServiceProvider.GetRequiredService<IDeliveryScheduleCacheRepository>()
			.InvalidateAsync(supplier, StorageCode, CancellationToken);
		return Scope.ServiceProvider.GetRequiredService<IDeliveryEstimator>();
	}
}
