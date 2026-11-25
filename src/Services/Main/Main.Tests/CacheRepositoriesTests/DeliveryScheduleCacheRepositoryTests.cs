using Enums;
using FluentAssertions;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Commands.DeliverySchedules;
using Main.Modules.Logistics.Application.Commands.DeliverySchedules.CreateDeliverySchedule;
using Main.Modules.Logistics.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.CacheRepositoriesTests;

public class DeliveryScheduleCacheRepositoryTests : IntegrationTest
{
	public DeliveryScheduleCacheRepositoryTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<StorageTestContext>();

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	private DateOnly Today
		=> DateOnly.FromDateTime(Scope.ServiceProvider.GetRequiredService<TestTimeProvider>().GetUtcNow().UtcDateTime);

	[Fact]
	public async Task Get_MissingSchedule_ReturnsNull()
	{
		await GetRepository().InvalidateAsync(Supplier.Armtek, StorageCode, CancellationToken);

		var result = await GetRepository().GetAsync(Supplier.Armtek, StorageCode, CancellationToken);

		result.Should().BeNull();
	}

	[Fact]
	public async Task Create_InvalidatesCache()
	{
		await GetRepository().InvalidateAsync(Supplier.Armtek, StorageCode, CancellationToken);
		(await GetRepository().GetAsync(Supplier.Armtek, StorageCode, CancellationToken)).Should().BeNull();

		await CreateAsync(DeliveryDays.Monday);

		var result = await GetRepository().GetAsync(Supplier.Armtek, $" {StorageCode} ", CancellationToken);
		result.Should().NotBeNull();
		result!.Days.Should().Be(DeliveryDays.Monday);
	}

	[Fact]
	public async Task UpdateAndExceptions_InvalidateCache()
	{
		await CreateAsync(DeliveryDays.Monday);
		await GetRepository().GetAsync(Supplier.Armtek, StorageCode, CancellationToken);
		var date = Today.AddDays(2);

		await Mediator.Send(
			new UpdateDeliveryScheduleCommand(Supplier.Armtek, StorageCode, DeliveryDays.Friday),
			CancellationToken);
		await Mediator.Send(
			new AddDeliveryScheduleExceptionCommand(Supplier.Armtek, StorageCode, date),
			CancellationToken);

		var result = await GetRepository().GetAsync(Supplier.Armtek, StorageCode, CancellationToken);
		result!.Days.Should().Be(DeliveryDays.Friday);
		result.Exceptions.Should().Equal(date);
	}

	[Fact]
	public async Task Delete_InvalidatesCache()
	{
		await CreateAsync(DeliveryDays.Monday);
		await GetRepository().GetAsync(Supplier.Armtek, StorageCode, CancellationToken);

		await Mediator.Send(new DeleteDeliveryScheduleCommand(Supplier.Armtek, StorageCode), CancellationToken);

		(await GetRepository().GetAsync(Supplier.Armtek, StorageCode, CancellationToken)).Should().BeNull();
	}

	[Fact]
	public async Task Update_InvalidatesOnlyChangedSchedule()
	{
		await CreateAsync(DeliveryDays.Monday);
		await Mediator.Send(
			new CreateDeliveryScheduleCommand(Supplier.Tmtr, StorageCode, DeliveryDays.Monday),
			CancellationToken);
		await GetRepository().GetAsync(Supplier.Armtek, StorageCode, CancellationToken);
		await GetRepository().GetAsync(Supplier.Tmtr, StorageCode, CancellationToken);

		await Context.Set<DeliverySchedule>()
			.Where(x => x.Supplier == Supplier.Tmtr)
			.ExecuteUpdateAsync(x => x.SetProperty(e => e.Days, DeliveryDays.Sunday), CancellationToken);
		await Mediator.Send(
			new UpdateDeliveryScheduleCommand(Supplier.Armtek, StorageCode, DeliveryDays.Friday),
			CancellationToken);

		(await GetRepository().GetAsync(Supplier.Armtek, StorageCode, CancellationToken))!
			.Days.Should().Be(DeliveryDays.Friday);
		(await GetRepository().GetAsync(Supplier.Tmtr, StorageCode, CancellationToken))!
			.Days.Should().Be(DeliveryDays.Monday);
	}

	private Task CreateAsync(DeliveryDays days)
		=> Mediator.Send(new CreateDeliveryScheduleCommand(Supplier.Armtek, StorageCode, days), CancellationToken);

	private IDeliveryScheduleCacheRepository GetRepository()
		=> Scope.ServiceProvider.GetRequiredService<IDeliveryScheduleCacheRepository>();
}
