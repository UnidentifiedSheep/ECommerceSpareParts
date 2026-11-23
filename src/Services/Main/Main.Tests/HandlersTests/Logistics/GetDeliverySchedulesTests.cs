using Application.Common.Models;
using Enums;
using FluentAssertions;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Queries.DeliverySchedules.GetDeliverySchedules;
using Tests.DataBuilders.Logistics;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Logistics;

public sealed class GetDeliverySchedulesTests(ReadScenarioFixture<GetDeliverySchedulesTests.DeliverySchedulesScenario> fixture)
	: ReadScenarioTest<GetDeliverySchedulesTests.DeliverySchedulesScenario>(fixture)
{
	[Fact]
	public async Task NoFilters_ReturnsSchedulesOrderedBySupplierAndStorage()
	{
		var result = await Mediator.Send(CreateQuery([], null), CancellationToken);

		result.Schedules
			.Select(x => (x.Supplier, x.StorageCode))
			.Should()
			.Equal(Scenario.Schedules.OrderBy(x => x.Supplier).ThenBy(x => x.StorageCode).Select(x => (x.Supplier, x.StorageCode)));
	}

	[Fact]
	public async Task SupplierAndStorageFilters_ReturnMatchingSchedule()
	{
		var expected = Scenario.ArmtekNorth;

		var result = await Mediator.Send(
			CreateQuery([Supplier.Armtek], $" {expected.StorageCode} "),
			CancellationToken);

		var schedule = result.Schedules.Should().ContainSingle().Subject;
		schedule.Supplier.Should().Be(expected.Supplier);
		schedule.StorageCode.Should().Be(expected.StorageCode);
		schedule.Days.Should().Be(expected.Days);
		schedule.Exceptions.Should().Equal(expected.Exceptions.Select(x => x.Date).Order());
	}

	[Fact]
	public async Task Pagination_ReturnsRequestedPage()
	{
		var ordered = Scenario.Schedules.OrderBy(x => x.Supplier).ThenBy(x => x.StorageCode).ToArray();

		var result = await Mediator.Send(CreateQuery([], null, 1, 2), CancellationToken);

		result.Schedules.Select(x => x.StorageCode).Should().Equal(ordered.Skip(2).Take(2).Select(x => x.StorageCode));
	}

	private static GetDeliverySchedulesQuery CreateQuery(
		IReadOnlyCollection<Supplier> suppliers,
		string? storageCode,
		int page = 0,
		int size = 20)
		=> new(suppliers, storageCode, new Pagination(page, size));

	public sealed class DeliverySchedulesScenario : ReadScenario
	{
		public DeliverySchedulesScenario() => RegisterBasicContext<StorageTestContext>();

		public DeliverySchedule ArmtekNorth { get; private set; } = null!;

		public IReadOnlyList<DeliverySchedule> Schedules { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			var storages = Get<StorageTestContext>().Storages.Select(x => x.Code).ToArray();
			ArmtekNorth = new DeliveryScheduleBuilder(Faker)
				.WithSupplier(Supplier.Armtek)
				.WithStorageCode(storages[0])
				.WithDays(DeliveryDays.Monday | DeliveryDays.Thursday)
				.WithException(new DateOnly(2030, 1, 10))
				.WithException(new DateOnly(2030, 1, 3))
				.Build();
			Schedules =
			[
				ArmtekNorth,
				Schedule(Supplier.Armtek, storages[1]),
				Schedule(Supplier.FavoritParts, storages[0]),
				Schedule(Supplier.FavoritParts, storages[1])
			];
			return Schedules;
		}

		private DeliverySchedule Schedule(Supplier supplier, string storageCode)
			=> new DeliveryScheduleBuilder(Faker).WithSupplier(supplier).WithStorageCode(storageCode).Build();
	}
}
