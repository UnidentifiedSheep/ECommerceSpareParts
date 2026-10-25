using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.User;
using Tests.TestContainers.Combined;

namespace Tests.Persistence;

public sealed class DatabaseMigrationsTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task ResetDataStores_PreservesMigrationHistoryAndAllowsReusingDatabase()
	{
		var migrations = Context.Database.GetMigrations().ToArray();
		migrations.Should().NotBeEmpty();
		(await Context.Database.GetAppliedMigrationsAsync(CancellationToken)).Should().Equal(migrations);

		var user = new UserBuilder(Faker).WithUserName("before-reset").Build();
		Context.Add(user);
		await Context.SaveChangesAsync(CancellationToken);

		await ResetDataStoresAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		(await Context.Users.AnyAsync(CancellationToken)).Should().BeFalse();
		(await Context.Database.GetAppliedMigrationsAsync(CancellationToken)).Should().Equal(migrations);
		(await Context.Database.GetPendingMigrationsAsync(CancellationToken)).Should().BeEmpty();
		await Context.Database.MigrateAsync(CancellationToken);

		Context.Add(new UserBuilder(Faker).WithUserName("after-reset").Build());
		await Context.SaveChangesAsync(CancellationToken);
		(await Context.Users.CountAsync(CancellationToken)).Should().Be(1);
	}
}
