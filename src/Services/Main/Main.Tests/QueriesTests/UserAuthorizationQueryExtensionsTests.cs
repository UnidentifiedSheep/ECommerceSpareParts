using Application.Common.Interfaces.Projections;
using Enums;
using FluentAssertions;
using Main.Application.Dtos.Users;
using Main.Application.Extensions.QueryExtensions;
using Main.Entities.Auth;
using Main.Entities.User;
using Main.Modules.Currency.Persistence;
using Main.Modules.Product.Persistence;
using Main.Modules.Storage.Persistence;
using Main.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.Persistence.Interceptors;
using Tests.TestContainers.Combined;
using Role = Main.Entities.Auth.Role;

namespace Tests.QueriesTests;

public class UserAuthorizationQueryExtensionsTests(CombinedContainerFixture fixture)
	: IntegrationTest(fixture)
{
	[Fact]
	public async Task AuthorizationAndBatchRights_EachExecuteOneQueryWithoutTracking()
	{
		Context.AddRange(
			new Permission(PermissionCodes.USERS_INFO_GET),
			new Permission(PermissionCodes.USERS_GET));
		var role = Role.Create("query-role");
		role.AddPermission(Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET));
		Context.Add(role);
		var user = await new UserBuilder(Faker).WithUserName("query-user")
			.WithUserInfo(
				"Name",
				"Surname",
				"Description")
			.WithRole(role.Name)
			.BuildAndAddToDb(Context);
		Context.Add(
			UserPermission.Create(
				user.Id,
				Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET)));
		Context.Add(
			UserPermission.Create(user.Id, Permission.ToNormalizedPermission(PermissionCodes.USERS_GET)));
		await Context.SaveChangesAsync(CancellationToken);
		var counter = new QueryCounter();
		await using var readContext = CreateReadContext(counter);
		var repository = new ReadRepository<User, Guid>(readContext);
		var projection = Scope.ServiceProvider.GetRequiredService<IProjectionProvider<User, UserDto>>();

		var authorization = await repository.GetUserAuthorizationAsync(
			user.Id,
			projection,
			CancellationToken);

		counter.Reads.Should().Be(1);
		authorization.Should().NotBeNull();
		authorization!.User.Id.Should().Be(user.Id);
		authorization.User.UserName.Should().Be("query-user");
		authorization.User.NormalizedUserName.Should().Be("QUERY-USER");
		authorization.User
			.UserInfo
			.Should()
			.BeEquivalentTo(
				new UserInfoDto
				{
					Name = "Name", Surname = "Surname", Description = "Description"
				});
		authorization.Roles.Should().Equal(role.Name);
		authorization.Permissions
			.Should()
			.BeEquivalentTo(
				Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET),
				Permission.ToNormalizedPermission(PermissionCodes.USERS_GET));
		readContext.ChangeTracker.Entries().Should().BeEmpty();

		var batch = await repository.GetUsersRolesAndPermissionsAsync(
			[user.Id, Guid.NewGuid(), user.Id],
			CancellationToken);

		counter.Reads.Should().Be(2);
		batch.Should().ContainSingle();
		batch[user.Id].Roles.Should().BeEquivalentTo(authorization.Roles);
		batch[user.Id].Permissions.Should().BeEquivalentTo(authorization.Permissions);
		readContext.ChangeTracker.Entries().Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_DoesNotExecuteSqlAndMissingUserReturnsNull()
	{
		var counter = new QueryCounter();
		await using var readContext = CreateReadContext(counter);
		var repository = new ReadRepository<User, Guid>(readContext);
		var projection = Scope.ServiceProvider.GetRequiredService<IProjectionProvider<User, UserDto>>();

		(await repository.GetUsersRolesAndPermissionsAsync([], CancellationToken)).Should().BeEmpty();
		counter.Reads.Should().Be(0);

		(await repository.GetUserAuthorizationAsync(
				Guid.NewGuid(),
				projection,
				CancellationToken)).Should()
			.BeNull();
		(await repository.GetUserRolesAndPermissionsAsync(Guid.NewGuid(), CancellationToken)).Should()
			.BeNull();
		counter.Reads.Should().Be(2);
	}

	[Fact]
	public async Task UserWithoutProfileOrAssignments_ReturnsEmptyRightsAndNullableProfile()
	{
		var user = await new UserBuilder(Faker).BuildAndAddToDb(Context);
		await using var readContext = CreateReadContext(new QueryCounter());
		var repository = new ReadRepository<User, Guid>(readContext);
		var projection = Scope.ServiceProvider.GetRequiredService<IProjectionProvider<User, UserDto>>();

		var authorization = await repository.GetUserAuthorizationAsync(
			user.Id,
			projection,
			CancellationToken);

		authorization.Should().NotBeNull();
		authorization!.User.UserInfo.Should().BeNull();
		authorization.Roles.Should().BeEmpty();
		authorization.Permissions.Should().BeEmpty();
	}

	private UnifiedDbContext CreateReadContext(QueryCounter counter)
		=> new(
			new DbContextOptionsBuilder<UnifiedDbContext>()
				.UseNpgsql(
					Context.Database.GetDbConnection(),
					options =>
						options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
				.AddInterceptors(counter)
				.Options,
			[new MainDbContextModule(), new Main.Modules.Identity.Persistence.IdentityDbContextModule(), new Main.Modules.Purchase.Persistence.PurchaseDbContextModule(), new Main.Modules.Sale.Persistence.SaleDbContextModule(), new Main.Modules.Organization.Persistence.OrganizationDbContextModule(), new Main.Modules.Balance.Persistence.BalanceDbContextModule(), new Main.Modules.Analytics.Persistence.AnalyticsDbContextModule(), new Main.Modules.Order.Persistence.OrderDbContextModule(), new Main.Modules.Notifications.Persistence.NotificationsDbContextModule(), new Main.Modules.Cart.Persistence.CartDbContextModule(), new Main.Modules.Documents.Persistence.DocumentsDbContextModule(), new Main.Modules.Jobs.Persistence.JobsDbContextModule(), new CurrencyDbContextModule(), new ProductDbContextModule(), new StorageDbContextModule(), new Pricing.Persistence.PricingDbContextModule()]);
}
