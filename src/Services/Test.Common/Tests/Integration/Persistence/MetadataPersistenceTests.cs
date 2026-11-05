using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Integration;
using Tests.Persistence.Entities;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Persistence;

public sealed class MetadataPersistenceTests(CombinedContainerFixture fixture) : CommonLayerIntegrationTest(fixture)
{
	[Fact]
	public async Task Metadata_RoundTripsAsJsonb()
	{
		var entity = new TestMetadataEntityBuilder().WithDescription("Поступление")
			.WithQuantity(12).WithTags("purchase", "manual").Build();
		Context.Add(entity);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var loaded = await Context.Set<TestMetadataEntity>().SingleAsync(CancellationToken);

		loaded.Metadata.Should().BeEquivalentTo(entity.Metadata);
		var columnType = await Context.Database.SqlQuery<string>(
			$"SELECT pg_typeof(metadata)::text AS \"Value\" FROM metadata_entities WHERE \"Id\" = {entity.Id}")
			.SingleAsync(CancellationToken);
		columnType.Should().Be("jsonb");
	}

	[Fact]
	public async Task Metadata_InPlaceChangesArePersisted()
	{
		var entity = new TestMetadataEntityBuilder().WithDescription("Original").WithQuantity(5).WithTags("first").Build();
		Context.Add(entity);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();
		var loaded = await Context.Set<TestMetadataEntity>().SingleAsync(CancellationToken);

		loaded.Metadata.Description = "Changed";
		loaded.Metadata.Details.Quantity = 7;
		loaded.Metadata.Tags.Add("second");
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var updated = await Context.Set<TestMetadataEntity>().SingleAsync(CancellationToken);
		updated.Metadata.Description.Should().Be("Changed");
		updated.Metadata.Details.Quantity.Should().Be(7);
		updated.Metadata.Tags.Should().Equal("first", "second");
	}

	[Fact]
	public async Task Metadata_ReplacementIsPersisted()
	{
		var entity = new TestMetadataEntityBuilder().WithDescription("Original").WithTags("old").Build();
		Context.Add(entity);
		await Context.SaveChangesAsync(CancellationToken);

		entity.ReplaceMetadata(new TestMetadata { Description = "Replacement" });
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var loaded = await Context.Set<TestMetadataEntity>().SingleAsync(CancellationToken);
		loaded.Metadata.Description.Should().Be("Replacement");
		loaded.Metadata.Tags.Should().BeEmpty();
	}

	[Fact]
	public async Task Metadata_PropertiesCanBeFilteredInSql()
	{
		var matching = new TestMetadataEntityBuilder().WithQuantity(10).Build();
		Context.AddRange(matching, new TestMetadataEntityBuilder().WithQuantity(2).Build());
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var ids = await Context.Set<TestMetadataEntity>()
			.Where(entity => entity.Metadata.Details.Quantity > 5)
			.Select(entity => entity.Id).ToListAsync(CancellationToken);

		ids.Should().Equal(matching.Id);
	}

	[Fact]
	public async Task AuditableEntity_DefaultMetadataIsPersistedAlongsideAuditFields()
	{
		var entity = new TestAuditableMetadataEntity();
		entity.Metadata.Description = "Audited";
		Context.Add(entity);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var loaded = await Context.Set<TestAuditableMetadataEntity>().SingleAsync(CancellationToken);
		loaded.Metadata.Description.Should().Be("Audited");
		loaded.CreatedAt.Should().BeCloseTo(entity.CreatedAt, TimeSpan.FromMilliseconds(1));
	}
}
