using FluentAssertions;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.Persistence;

public sealed class StorageContentMetadataTests : IntegrationTest
{
	public StorageContentMetadataTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<StorageContentTestContext>();

	[Fact]
	public async Task Metadata_MovementTypeSurvivesReload()
	{
		var content = GetContext<StorageContentTestContext>().StorageContents.First();

		var loaded = await Context.StorageContents.AsNoTracking()
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

		var loaded = await Context.StorageContents.AsNoTracking()
			.SingleAsync(item => item.Id == content.Id, CancellationToken);

		loaded.Metadata.LastMovementType.Should().BeNull();
	}
}
