using Enums;
using FluentAssertions;
using Main.Entities.Storage;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.Persistence;

public sealed class StorageContentMetadataTests : IntegrationTest
{
	public StorageContentMetadataTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<StorageContentTestContext>();

	[Fact]
	public async Task Metadata_MovementTypeSurvivesReload()
	{
		var content = GetContext<StorageContentTestContext>().StorageContents.First();

		var loaded = await Context.Set<StorageContent>()
			.AsNoTracking()
			.SingleAsync(item => item.Id == content.Id, CancellationToken);

		loaded.Metadata.LastMovementType.Should().Be(StorageMovementType.StorageContentAddition);
	}

	[Fact]
	public async Task Metadata_EmptyDocumentFromMigrationHasUnknownMovementType()
	{
		var content = GetContext<StorageContentTestContext>().StorageContents.First();
		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"UPDATE public.storage_content SET metadata = '{{}}'::jsonb WHERE id = {content.Id}",
			CancellationToken);

		var loaded = await Context.Set<StorageContent>()
			.AsNoTracking()
			.SingleAsync(item => item.Id == content.Id, CancellationToken);

		loaded.Metadata.LastMovementType.Should().BeNull();
	}
}
