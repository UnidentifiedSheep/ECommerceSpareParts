using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Auth.RemoveRolesFromUser;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Auth;

public class RemoveRolesFromUserTests : IntegrationTest
{
	public RemoveRolesFromUserTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UsersTestContext>();
	}

	private UsersTestContext TestContext => GetContext<UsersTestContext>();

	[Fact]
	public async Task RemoveRoles_RemovesOnlyRequestedAssignmentsAndReturnsTheirNames()
	{
		var user = TestContext.User;
		var reader = TestContext.ReaderRole.Name;
		var editor = TestContext.EditorRole.Name;
		user.AddRole(editor);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(
			new RemoveRolesFromUserCommand(user.Id, [$" {reader.ToLowerInvariant()} ", editor, reader]), CancellationToken);

		result.UserId.Should().Be(user.Id);
		result.RemovedRoles.Should().Equal(reader, editor);
		(await RoleNamesFor(user.Id)).Should().Equal("MEMBER");
		(await RoleNamesFor(TestContext.OtherUser.Id)).Should().BeEquivalentTo(["MEMBER", editor]);
	}

	[Fact]
	public async Task RemoveRoles_MissingAssignmentRejectsWholeBatch()
	{
		var action = () => Mediator.Send(new RemoveRolesFromUserCommand(
			TestContext.User.Id, [TestContext.ReaderRole.Name, TestContext.EditorRole.Name]), CancellationToken);

		await action.Should().ThrowAsync<UserRoleNotFoundException>();

		(await RoleNamesFor(TestContext.User.Id)).Should().BeEquivalentTo(["MEMBER", TestContext.ReaderRole.Name]);
	}

	[Fact]
	public async Task SystemRoleInBatch_IsRejectedWithoutChangingOtherRoles()
	{
		var action = () => Mediator.Send(new RemoveRolesFromUserCommand(TestContext.User.Id,
			[TestContext.ReaderRole.Name, " system "]), CancellationToken);

		await action.Should().ThrowAsync<ValidationException>();

		(await RoleNamesFor(TestContext.User.Id)).Should().BeEquivalentTo(["MEMBER", TestContext.ReaderRole.Name]);
	}

	[Fact]
	public async Task MissingUser_IsRejected()
	{
		var action = () => Mediator.Send(
			new RemoveRolesFromUserCommand(Guid.NewGuid(), [TestContext.ReaderRole.Name]), CancellationToken);

		await action.Should().ThrowAsync<DbValidationException>();
	}

	private Task<string[]> RoleNamesFor(Guid userId) => Context.Set<UserRole>().AsNoTracking()
		.Where(x => x.UserId == userId).Select(x => x.RoleName).ToArrayAsync(CancellationToken);
}
