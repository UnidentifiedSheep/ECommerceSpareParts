using Main.Entities.Storage.Exceptions;
using Main.Entities.Storage.Storage;
using Main.Modules.Storage.Application.Commands.Storages.DeleteStorage;
using Microsoft.EntityFrameworkCore;
using Tests.Abstractions.Test;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Storages;

public class DeleteStorageTests : IntegrationTest
{
	public DeleteStorageTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<StorageContentTestContext>();

	[Fact]
	public async Task DeleteStorage_NotExisting_ThrowsStorageNotFoundException()
	{
		var command = new DeleteStorageCommand("some-non-existing-storage");
		await Assert.ThrowsAsync<StorageNotFoundException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task DeleteStorage_WithNoContents_Succeeds()
	{
		var storage = GetContext<StorageTestContext>()
			.Storages
			.First(x =>
				x.Code != GetContext<StorageContentTestContext>().StorageContents.First().StorageCode);
		var command = new DeleteStorageCommand(storage.Code);
		await Mediator.Send(command, CancellationToken);

		var exists = await Context.Set<Storage>().AnyAsync(x => x.Code == storage.Code, CancellationToken);
		Assert.False(exists);
	}

	[Fact]
	public async Task DeleteStorage_WithContents_ThrowsDbUpdateException()
	{
		var storage = GetContext<StorageContentTestContext>().StorageContents.First().StorageCode;

		var command = new DeleteStorageCommand(storage);
		await Assert.ThrowsAsync<DbUpdateException>(async () =>
			await Mediator.Send(command, CancellationToken));

		var exists = await Context.Set<Storage>().AnyAsync(x => x.Code == storage, CancellationToken);
		Assert.True(exists);
	}
}

public class DeleteStorageValidationTests : ValidationTest<DeleteStorageValidation, DeleteStorageCommand>
{
	[Fact]
	public async Task DeleteStorage_EmptyName_FailsValidation()
	{
		var command = new DeleteStorageCommand("");
		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor(x => x.StorageCode);
	}
}
