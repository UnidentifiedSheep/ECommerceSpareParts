using Application.Common.Models;
using Enums;
using FluentAssertions;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Exceptions;
using Main.Entities.Logistics.Schedule;
using Main.Entities.Storage.Exceptions;
using Main.Modules.Logistics.Application.Commands.DeliverySchedules;
using Main.Modules.Logistics.Application.Commands.DeliverySchedules.CreateDeliverySchedule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Test;
using Tests.DataBuilders.Logistics;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Logistics;

public sealed class DeliveryScheduleCommandsTests : IntegrationTest
{
	public DeliveryScheduleCommandsTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<StorageTestContext>();

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	private DateOnly Today
		=> DateOnly.FromDateTime(Scope.ServiceProvider.GetRequiredService<TestTimeProvider>().GetUtcNow().UtcDateTime);

	[Fact]
	public async Task Create_ExistingStorage_PersistsSchedule()
	{
		await Mediator.Send(
			new CreateDeliveryScheduleCommand(
				Supplier.Armtek,
				$"  {StorageCode}  ",
				DeliveryDays.Monday | DeliveryDays.Thursday),
			CancellationToken);

		var schedule = await LoadAsync(Supplier.Armtek, StorageCode);
		schedule.Should().NotBeNull();
		schedule.Days.Should().Be(DeliveryDays.Monday | DeliveryDays.Thursday);
	}

	[Fact]
	public async Task Create_ExistingSchedule_ThrowsAlreadyExists()
	{
		await CreateScheduleAsync();

		var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(
			new CreateDeliveryScheduleCommand(Supplier.Armtek, StorageCode, DeliveryDays.Friday),
			CancellationToken));

		exception.Failures.Should().Contain(x => x.Error is DeliveryScheduleAlreadyExistsException);
	}

	[Fact]
	public async Task Create_UnknownStorage_ThrowsStorageNotFound()
	{
		var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(
			new CreateDeliveryScheduleCommand(Supplier.Armtek, "missing-storage", DeliveryDays.Friday),
			CancellationToken));

		exception.Failures.Should().Contain(x => x.Error is StorageNotFoundException);
	}

	[Fact]
	public async Task Update_ExistingSchedule_ReplacesDays()
	{
		await CreateScheduleAsync();

		await Mediator.Send(
			new UpdateDeliveryScheduleCommand(Supplier.Armtek, StorageCode, DeliveryDays.Saturday),
			CancellationToken);

		(await LoadAsync(Supplier.Armtek, StorageCode))!.Days.Should().Be(DeliveryDays.Saturday);
	}

	[Fact]
	public async Task Update_MissingSchedule_ThrowsNotFound()
	{
		var act = () => Mediator.Send(
			new UpdateDeliveryScheduleCommand(Supplier.Armtek, StorageCode, DeliveryDays.Saturday),
			CancellationToken);

		await act.Should().ThrowAsync<DeliveryScheduleNotFoundException>();
	}

	[Fact]
	public async Task Delete_ScheduleWithExceptions_RemovesScheduleAndExceptions()
	{
		await CreateScheduleAsync(Today.AddDays(3));

		await Mediator.Send(new DeleteDeliveryScheduleCommand(Supplier.Armtek, StorageCode), CancellationToken);

		(await LoadAsync(Supplier.Armtek, StorageCode)).Should().BeNull();
		(await Context.Set<DeliveryScheduleException>().AnyAsync(CancellationToken)).Should().BeFalse();
	}

	[Fact]
	public async Task AddException_FutureDate_PersistsException()
	{
		await CreateScheduleAsync();
		var date = Today.AddDays(2);

		await Mediator.Send(
			new AddDeliveryScheduleExceptionCommand(Supplier.Armtek, StorageCode, date),
			CancellationToken);

		(await LoadAsync(Supplier.Armtek, StorageCode))!.Exceptions.Select(x => x.Date).Should().Equal(date);
	}

	[Fact]
	public async Task AddException_ExistingDate_ThrowsAlreadyExists()
	{
		var date = Today.AddDays(2);
		await CreateScheduleAsync(date);

		var act = () => Mediator.Send(
			new AddDeliveryScheduleExceptionCommand(Supplier.Armtek, StorageCode, date),
			CancellationToken);

		await act.Should().ThrowAsync<ScheduleExceptionAlreadyExistsException>();
	}

	[Fact]
	public async Task AddException_PastDate_Throws()
	{
		await CreateScheduleAsync();

		var act = () => Mediator.Send(
			new AddDeliveryScheduleExceptionCommand(Supplier.Armtek, StorageCode, Today.AddDays(-1)),
			CancellationToken);

		await act.Should().ThrowAsync<Exception>();
		(await LoadAsync(Supplier.Armtek, StorageCode))!.Exceptions.Should().BeEmpty();
	}

	[Fact]
	public async Task RemoveException_ExistingDate_RemovesException()
	{
		var date = Today.AddDays(2);
		await CreateScheduleAsync(date, Today.AddDays(4));

		await Mediator.Send(
			new RemoveDeliveryScheduleExceptionCommand(Supplier.Armtek, StorageCode, date),
			CancellationToken);

		(await LoadAsync(Supplier.Armtek, StorageCode))!
			.Exceptions
			.Select(x => x.Date)
			.Should()
			.Equal(Today.AddDays(4));
	}

	[Fact]
	public async Task RemoveException_MissingDate_ThrowsNotFound()
	{
		await CreateScheduleAsync();

		var act = () => Mediator.Send(
			new RemoveDeliveryScheduleExceptionCommand(Supplier.Armtek, StorageCode, Today.AddDays(2)),
			CancellationToken);

		await act.Should().ThrowAsync<ScheduleExceptionNotFoundException>();
	}

	private async Task CreateScheduleAsync(params DateOnly[] exceptions)
	{
		var builder = new DeliveryScheduleBuilder(Faker)
			.WithSupplier(Supplier.Armtek)
			.WithStorageCode(StorageCode)
			.WithDays(DeliveryDays.Monday);
		foreach (var date in exceptions)
			builder.WithException(date);
		await builder.BuildAndAddToDb(Context, CancellationToken);
		Context.ChangeTracker.Clear();
	}

	private Task<DeliverySchedule?> LoadAsync(Supplier supplier, string storageCode)
		=> Context.Set<DeliverySchedule>()
			.AsNoTracking()
			.Include(x => x.Exceptions)
			.FirstOrDefaultAsync(x => x.Supplier == supplier && x.StorageCode == storageCode, CancellationToken);
}

public class CreateDeliveryScheduleValidationTests
	: ValidationTest<CreateDeliveryScheduleValidation, CreateDeliveryScheduleCommand>
{
	[Theory]
	[InlineData("", DeliveryDays.Monday, nameof(CreateDeliveryScheduleCommand.StorageCode))]
	[InlineData("   ", DeliveryDays.Monday, nameof(CreateDeliveryScheduleCommand.StorageCode))]
	[InlineData("storage", DeliveryDays.None, nameof(CreateDeliveryScheduleCommand.Days))]
	[InlineData("storage", (DeliveryDays)128, nameof(CreateDeliveryScheduleCommand.Days))]
	public async Task InvalidCommand_FailsValidation(string storageCode, DeliveryDays days, string property)
	{
		var result = await ValidateAsync(new CreateDeliveryScheduleCommand(Supplier.Armtek, storageCode, days));

		result.ShouldHaveValidationErrorFor(property);
	}

	[Fact]
	public async Task TooLongStorageCode_FailsValidation()
	{
		var command = new CreateDeliveryScheduleCommand(
			Supplier.Armtek,
			new string('a', DeliverySchedule.StorageCodeMaxLength + 1),
			DeliveryDays.Monday);

		var result = await ValidateAsync(command);

		result.ShouldHaveValidationErrorFor(x => x.StorageCode);
	}
}
