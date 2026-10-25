using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Auth.AddPermissionsToUser;
using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Auth;

public class AddPermissionsToUserTests : IntegrationTest
{
	public AddPermissionsToUserTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UsersTestContext>();
	}

	private UsersTestContext TestContext => GetContext<UsersTestContext>();

	[Fact]
	public async Task AddPermissions_NormalizesAndDeduplicatesNamesAndReturnsOnlyAddedPermissions()
	{
		var command = new AddPermissionsToUserCommand(
			TestContext.User.Id,
			[TestContext.ReadPermission.ToLowerInvariant(),
				" users_create ",
				"USERS.CREATE"]);

		var result = await Mediator.Send(command, CancellationToken);
		var repeated = await Mediator.Send(command, CancellationToken);

		result.UserId.Should().Be(TestContext.User.Id);
		result.AddedPermissions.Should().Equal(TestContext.CreatePermission);
		repeated.AddedPermissions.Should().BeEmpty();
		(await PermissionsFor(TestContext.User.Id)).Should().BeEquivalentTo([
			TestContext.ReadPermission, TestContext.InfoPermission, TestContext.CreatePermission]);
		(await PermissionsFor(TestContext.OtherUser.Id)).Should().Equal(TestContext.CreatePermission);
	}

	[Fact]
	public async Task AddPermissions_UnknownPermissionRejectsWholeBatch()
	{
		var action = () => Mediator.Send(new AddPermissionsToUserCommand(TestContext.User.Id,
			[TestContext.CreatePermission, "UNKNOWN.PERMISSION"]), CancellationToken);

		await action.Should().ThrowAsync<DbValidationException>();

		(await PermissionsFor(TestContext.User.Id)).Should().BeEquivalentTo([
			TestContext.ReadPermission, TestContext.InfoPermission]);
	}

	[Fact]
	public async Task MissingUser_IsRejected()
	{
		var action = () => Mediator.Send(
			new AddPermissionsToUserCommand(Guid.NewGuid(), [TestContext.CreatePermission]), CancellationToken);

		await action.Should().ThrowAsync<DbValidationException>();
	}

	private Task<string[]> PermissionsFor(Guid userId) => Context.Set<UserPermission>().AsNoTracking()
		.Where(x => x.UserId == userId).Select(x => x.Permission).ToArrayAsync(CancellationToken);
}
