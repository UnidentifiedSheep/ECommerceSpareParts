using Enums;
using FluentAssertions;
using Main.Application.Handlers.Users;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Role = Main.Entities.Auth.Role;

namespace Tests.HandlersTests.Users;

public class GetUsersRolesAndPermissionsTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task Batch_CombinesDirectAndRolePermissionsWithoutDuplicatesOrMixingUsers()
	{
		Context.AddRange(
			new Permission(PermissionCodes.USERS_INFO_GET),
			new Permission(PermissionCodes.USERS_ROLES_CREATE),
			new Permission(PermissionCodes.USERS_PERMISSIONS_CREATE),
			new Permission(PermissionCodes.USERS_GET));
		var role = Role.Create("batch-first");
		role.AddPermission(Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET));
		var otherRole = Role.Create("batch-second");
		otherRole.AddPermission(Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET));
		otherRole.AddPermission(Permission.ToNormalizedPermission(PermissionCodes.USERS_GET));
		Context.AddRange(role, otherRole);
		var first = await new UserBuilder(Faker).WithUserName("batch-first-user")
			.WithRole(role.Name).WithRole(otherRole.Name).BuildAndAddToDb(Context);
		var second = await new UserBuilder(Faker).WithUserName("batch-second-user").BuildAndAddToDb(Context);
		Context.Add(UserPermission.Create(first.Id, Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET)));
		Context.Add(UserPermission.Create(first.Id, Permission.ToNormalizedPermission(PermissionCodes.USERS_ROLES_CREATE)));
		Context.Add(UserPermission.Create(second.Id, Permission.ToNormalizedPermission(PermissionCodes.USERS_PERMISSIONS_CREATE)));
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(
			new GetUsersRolesAndPermissionsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Users.Should().HaveCount(2);
		result.Users[first.Id].Roles.Should().BeEquivalentTo([role.Name, otherRole.Name]);
		result.Users[first.Id].Permissions.Should().BeEquivalentTo([
			Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET),
			Permission.ToNormalizedPermission(PermissionCodes.USERS_ROLES_CREATE),
			Permission.ToNormalizedPermission(PermissionCodes.USERS_GET)]);
		result.Users[second.Id].Roles.Should().BeEmpty();
		result.Users[second.Id].Permissions.Should().Equal(Permission.ToNormalizedPermission(PermissionCodes.USERS_PERMISSIONS_CREATE));
	}

	[Fact]
	public async Task RoleAndDirectPermissionChanges_AreVisibleOnNextRead()
	{
		Context.AddRange(new Permission(PermissionCodes.USERS_INFO_GET), new Permission(PermissionCodes.USERS_GET));
		var role = Role.Create("changing-role");
		role.AddPermission(Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET));
		Context.Add(role);
		var user = await new UserBuilder(Faker).WithRole(role.Name).BuildAndAddToDb(Context);
		var direct = UserPermission.Create(user.Id, Permission.ToNormalizedPermission(PermissionCodes.USERS_GET));
		Context.Add(direct);
		await Context.SaveChangesAsync(CancellationToken);
		var query = new GetUsersRolesAndPermissionsQuery(user.Id);
		(await Mediator.Send(query, CancellationToken)).Users[user.Id].Permissions.Should().HaveCount(2);

		role.RemovePermission(Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET));
		Context.Remove(direct);
		await Context.SaveChangesAsync(CancellationToken);

		var updated = (await Mediator.Send(query, CancellationToken)).Users[user.Id];
		updated.Roles.Should().Equal(role.Name);
		updated.Permissions.Should().BeEmpty();
	}

	[Fact]
	public async Task ExistingUserWithoutRolesOrPermissions_ReturnsEmptyLists()
	{
		var user = await new UserBuilder(Faker).BuildAndAddToDb(Context);

		var result = await Mediator.Send(new GetUsersRolesAndPermissionsQuery(user.Id), CancellationToken);

		result.Users[user.Id].Roles.Should().BeEmpty();
		result.Users[user.Id].Permissions.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingSingleUser_ThrowsUserNotFound()
	{
		var action = () => Mediator.Send(new GetUsersRolesAndPermissionsQuery(Guid.NewGuid()), CancellationToken);

		await action.Should().ThrowAsync<UserNotFoundException>();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetUsersRolesAndPermissionsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Users.Should().BeEmpty();
	}
}
