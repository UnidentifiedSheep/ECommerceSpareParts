using FluentAssertions;
using Main.Application.Dtos.Emails;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Users.CreateUser;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Entities.Organization;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Main.Enums;
using Main.Enums.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Security.Core.Interfaces;
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

	private static CreateUserCommand CreateCommand(IReadOnlyCollection<string> roles) => new(
		"created-user",
		"TestPassword123!",
		new UserInfoDto { Name = "Created", Surname = "User", Description = null },
		[new EmailDto { Email = "created@example.com", Type = EmailType.Work, IsPrimary = true, IsConfirmed = true }],
		[new UserPhoneDto { Number = "+1 (555) 123-4567", Type = PhoneType.Mobile, IsPrimary = true, IsConfirmed = true }],
		roles);
}
