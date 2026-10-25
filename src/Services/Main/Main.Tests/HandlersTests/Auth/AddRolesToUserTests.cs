using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Auth.AddRolesToUser;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Auth;

public class AddRolesToUserTests : IntegrationTest
{
	public AddRolesToUserTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UserAccessTestContext>();
	}

	private UserAccessTestContext TestContext => GetContext<UserAccessTestContext>();

	[Fact]
	public async Task AddRoles_NormalizesAndDeduplicatesNamesAndReturnsOnlyAddedRoles()
	{
		var userId = TestContext.User.Id;
		var reader = TestContext.ReaderRole.Name;
		var editor = TestContext.EditorRole.Name;
		var command = new AddRolesToUserCommand(userId, [reader.ToLowerInvariant(), $" {editor} ", editor]);

		var result = await Mediator.Send(command, CancellationToken);
		var repeated = await Mediator.Send(command, CancellationToken);

		result.UserId.Should().Be(userId);
		result.AddedRoles.Should().Equal(editor);
		repeated.AddedRoles.Should().BeEmpty();
		(await RoleNamesFor(userId)).Should().BeEquivalentTo(["MEMBER", reader, editor]);
		(await RoleNamesFor(TestContext.OtherUser.Id)).Should().BeEquivalentTo(["MEMBER", editor]);
	}

	[Fact]
	public async Task AddRoles_UnknownRoleRejectsWholeBatch()
	{
		var action = () => Mediator.Send(new AddRolesToUserCommand(
			TestContext.User.Id, [TestContext.EditorRole.Name, "missing-role"]), CancellationToken);

		await action.Should().ThrowAsync<RoleNotFoundException>();

		(await RoleNamesFor(TestContext.User.Id)).Should().BeEquivalentTo(["MEMBER", TestContext.ReaderRole.Name]);
	}

	[Fact]
	public async Task SystemRoleInBatch_IsRejectedWithoutChangingOtherRoles()
	{
		var action = () => Mediator.Send(new AddRolesToUserCommand(TestContext.User.Id,
			[TestContext.EditorRole.Name, " system "]), CancellationToken);

		await action.Should().ThrowAsync<ValidationException>();

		(await RoleNamesFor(TestContext.User.Id)).Should().BeEquivalentTo(["MEMBER", TestContext.ReaderRole.Name]);
	}

	[Fact]
	public async Task MissingUser_IsRejected()
	{
		var action = () => Mediator.Send(
			new AddRolesToUserCommand(Guid.NewGuid(), [TestContext.EditorRole.Name]), CancellationToken);

		await action.Should().ThrowAsync<DbValidationException>();
	}

	private Task<string[]> RoleNamesFor(Guid userId) => Context.Set<UserRole>().AsNoTracking()
		.Where(x => x.UserId == userId).Select(x => x.RoleName).ToArrayAsync(CancellationToken);
}
