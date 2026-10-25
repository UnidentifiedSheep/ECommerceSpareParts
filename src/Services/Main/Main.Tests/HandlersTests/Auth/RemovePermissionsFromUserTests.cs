using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Auth.RemovePermissionsFromUser;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Auth;

public class RemovePermissionsFromUserTests : IntegrationTest
{
	public RemovePermissionsFromUserTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UserAccessTestContext>();
	}

	private UserAccessTestContext TestContext => GetContext<UserAccessTestContext>();

	[Fact]
	public async Task RemovePermissions_RemovesDirectPermissionsButPreservesRolePermissionsAndOtherUsers()
	{
		var result = await Mediator.Send(new RemovePermissionsFromUserCommand(TestContext.User.Id,
			[" users_get ", TestContext.InfoPermission.ToLowerInvariant(), "USERS.GET"]), CancellationToken);

		result.UserId.Should().Be(TestContext.User.Id);
		result.RemovedPermissions.Should().Equal(TestContext.ReadPermission, TestContext.InfoPermission);
		(await PermissionsFor(TestContext.User.Id)).Should().BeEmpty();
		(await PermissionsFor(TestContext.OtherUser.Id)).Should().Equal(TestContext.CreatePermission);
		var inherited = await Context.Set<RolePermission>().AsNoTracking()
			.Where(x => x.RoleName == TestContext.ReaderRole.Name)
			.Select(x => x.PermissionName).ToArrayAsync(CancellationToken);
		inherited.Should().Equal(TestContext.InfoPermission);
	}

	[Fact]
	public async Task RemovePermissions_MissingAssignmentRejectsWholeBatch()
	{
		var action = () => Mediator.Send(new RemovePermissionsFromUserCommand(TestContext.User.Id,
			[TestContext.ReadPermission, TestContext.CreatePermission]), CancellationToken);

		await action.Should().ThrowAsync<UserPermissionNotFound>();

		(await PermissionsFor(TestContext.User.Id)).Should().BeEquivalentTo([
			TestContext.ReadPermission, TestContext.InfoPermission]);
	}

	[Fact]
	public async Task MissingUser_IsRejected()
	{
		var action = () => Mediator.Send(
			new RemovePermissionsFromUserCommand(Guid.NewGuid(), [TestContext.ReadPermission]), CancellationToken);

		await action.Should().ThrowAsync<DbValidationException>();
	}

	private Task<string[]> PermissionsFor(Guid userId) => Context.Set<UserPermission>().AsNoTracking()
		.Where(x => x.UserId == userId).Select(x => x.Permission).ToArrayAsync(CancellationToken);
}
