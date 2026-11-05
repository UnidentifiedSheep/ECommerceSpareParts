using System.Text.Json;
using Domain.CommonEntities.History;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Integration;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Persistence;

public sealed class EntityHistoryPersistenceTests(CombinedContainerFixture fixture) : CommonLayerIntegrationTest(fixture)
{
	[Fact]
	public async Task History_RoundTripsChangesAndDistinctReasonsAsJsonb()
	{
		EntityHistory history;
		var reasons = new[]
		{
			new EntityHistoryReason("Sale", "Sale", "10"),
			new EntityHistoryReason("Sale", "Sale", "10"),
			new EntityHistoryReason("Sale", "Sale", "11"),
			new EntityHistoryReason("InventoryCorrection")
		};
		using (var changes = JsonDocument.Parse("""{"Count":{"old":15,"new":10}}"""))
			history = new EntityHistoryBuilder().WithChanges(changes.RootElement).WithReasons(reasons).Build();
		reasons[0] = new EntityHistoryReason("ChangedByCaller");

		Context.Add(history);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var loaded = await Context.Set<EntityHistory>().SingleAsync(CancellationToken);
		loaded.Should().BeEquivalentTo(history, options => options
			.Excluding(item => item.Changes).Excluding(item => item.OccurredAt));
		loaded.OccurredAt.Should().BeCloseTo(history.OccurredAt, TimeSpan.FromMicroseconds(1));
		loaded.Changes.GetProperty("Count").GetProperty("old").GetInt32().Should().Be(15);
		loaded.Changes.GetProperty("Count").GetProperty("new").GetInt32().Should().Be(10);
		loaded.Reasons.Should().Equal(
			new EntityHistoryReason("Sale", "Sale", "10"),
			new EntityHistoryReason("Sale", "Sale", "11"),
			new EntityHistoryReason("InventoryCorrection"));
		var columnTypes = await Context.Database.SqlQuery<string>(
			$"SELECT pg_typeof(changes)::text || ',' || pg_typeof(reasons)::text AS \"Value\" FROM audit.entity_history WHERE id = {history.Id}")
			.SingleAsync(CancellationToken);
		columnTypes.Should().Be("jsonb,jsonb");
	}

	[Fact]
	public async Task History_WithoutReason_PersistsEmptyArray()
	{
		var history = new EntityHistoryBuilder().Build();
		Context.Add(history);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var loaded = await Context.Set<EntityHistory>().SingleAsync(CancellationToken);
		loaded.Reasons.Should().BeEmpty();
		var reasons = await Context.Database.SqlQuery<string>(
			$"SELECT reasons::text AS \"Value\" FROM audit.entity_history WHERE id = {history.Id}")
			.SingleAsync(CancellationToken);
		reasons.Should().Be("[]");
	}
}
