using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;

namespace Tests.Persistence;

public class ModelSnapshotTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public void UnifiedModel_HasNoPendingChangesComparedToSnapshot()
		=> Context.Database.HasPendingModelChanges().Should().BeFalse();
}
