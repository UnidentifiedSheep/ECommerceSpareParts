using FluentAssertions;
using Main.Application.Dtos.Emails;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Users.CreateUser;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Entities.Organization;
using Main.Entities.User.ValueObjects;
using Main.Entities.User;
using Main.Enums.Organization;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Security.Core.Interfaces;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Users;

public class CreateUserTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task CreateUser_CreatesUserWithHashedPasswordAndIndividualOrganization()
	{
		var command = CreateCommand([" member ", "MEMBER"]);

		var result = await Mediator.Send(command, CancellationToken);

		var user = await Context.Set<User>().AsNoTracking().SingleAsync(x => x.Id == result.UserId, CancellationToken);
		Scope.ServiceProvider.GetRequiredService<IPasswordManager>()
			.VerifyHashedPassword(user.PasswordHash, command.Password).Should().BeTrue();
		var roles = await Context.Set<UserRole>().AsNoTracking()
			.Where(x => x.UserId == user.Id).Select(x => x.RoleName).ToArrayAsync(CancellationToken);
		roles.Should().Equal("MEMBER");
		var organization = await Context.Set<Organization>().AsNoTracking()
			.Include(x => x.Members).SingleAsync(x => x.Id == user.Id, CancellationToken);
		organization.Type.Should().Be(OrganizationType.Individual);
		organization.Members.Should().ContainSingle(x => x.UserId == user.Id && x.Role == OrganizationRole.Owner);
	}

	[Theory]
	[InlineData("SYSTEM")]
	[InlineData("system")]
	[InlineData(" System ")]
	[InlineData("sYsTeM")]
	public async Task CreateUser_SystemRoleIsRejectedRegardlessOfCaseAndWhitespace(string systemRole)
	{
		var command = CreateCommand(["MEMBER", systemRole]);
		var action = () => Mediator.Send(command, CancellationToken);

		await action.Should().ThrowAsync<CantCreateSystemUserException>();

		var normalizedName = UserName.ToNormalized(command.UserName);
		(await Context.Set<User>().AsNoTracking().AnyAsync(
			x => x.UserName.NormalizedValue == normalizedName, CancellationToken)).Should().BeFalse();
	}

	[Fact]
	public async Task DuplicateNormalizedLogin_ReturnsLocalizedErrorWithoutCreatingUser()
	{
		await new UserBuilder(Faker).WithUserName("CREATED-USER").BuildAndAddToDb(Context);

		var exception = await Assert.ThrowsAsync<DbValidationException>(() =>
			Mediator.Send(CreateCommand(["MEMBER"]), CancellationToken));

		exception.Failures.Should().ContainSingle(failure => failure.Error is UserNameAlreadyTakenException);
		(await Context.Set<User>().CountAsync(
			user => user.UserName.NormalizedValue == "CREATED-USER", CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task EmailOwnedByAnotherUser_IsRejectedAfterNormalization()
	{
		await new UserBuilder(Faker).WithEmail("created@example.com").BuildAndAddToDb(Context);
		var command = CreateCommand(["MEMBER"]) with
		{
			Emails = [new EmailDto { Email = "CREATED@EXAMPLE.COM", Type = EmailType.Work }]
		};

		var exception = await Assert.ThrowsAsync<DbValidationException>(() =>
			Mediator.Send(command, CancellationToken));

		exception.Failures.Should().ContainSingle(failure => failure.Error is UserEmailAlreadyInUseException);
		(await Context.Set<User>().AnyAsync(
			user => user.UserName.NormalizedValue == "CREATED-USER", CancellationToken)).Should().BeFalse();
	}

	[Fact]
	public async Task MissingRole_IsRejectedWithoutCreatingUser()
	{
		var exception = await Assert.ThrowsAsync<DbValidationException>(() =>
			Mediator.Send(CreateCommand(["member", "missing-role"]), CancellationToken));

		exception.Failures.Should().ContainSingle(failure => failure.Error is RoleNotFoundException);
		(await Context.Set<User>().AnyAsync(
			user => user.UserName.NormalizedValue == "CREATED-USER", CancellationToken)).Should().BeFalse();
	}

	[Fact]
	public async Task EmptyRoles_CreatesUserWithoutAddingAnEmptyValidationCheck()
	{
		var result = await Mediator.Send(CreateCommand([]), CancellationToken);

		(await Context.Set<UserRole>().AnyAsync(role => role.UserId == result.UserId, CancellationToken))
			.Should().BeFalse();
	}

	private static CreateUserCommand CreateCommand(IReadOnlyCollection<string> roles) => new(
		"created-user",
		"TestPassword123!",
		new UserInfoDto { Name = "Created", Surname = "User", Description = null },
		[new EmailDto { Email = "created@example.com", Type = EmailType.Work, IsPrimary = true, IsConfirmed = true }],
		[new UserPhoneDto { Number = "+1 (555) 123-4567", Type = PhoneType.Mobile, IsPrimary = true, IsConfirmed = true }],
		roles);
}
