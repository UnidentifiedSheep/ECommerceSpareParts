using Abstractions.Models.Options;
using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Users.AddEmailToUser;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Users;

public class AddEmailToUserTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task AddEmail_ValidData_AddsUnconfirmedNonPrimaryEmail()
	{
		var user = await CreateUser();
		const string email = "Additional.Email@example.com";

		var created = await Mediator.Send(
			new AddEmailToUserCommand(
				user.Id,
				email,
				EmailType.Work));

		created.UserId.Should().Be(user.Id);
		created.Email.Should().Be("additional.email@example.com");

		var addedEmail = await Context.UserEmails.AsNoTracking().SingleAsync(x => x.Email == email);

		addedEmail.UserId.Should().Be(user.Id);
		addedEmail.Email.Value.Should().Be("additional.email@example.com");
		addedEmail.EmailType.Should().Be(EmailType.Work);
		addedEmail.Confirmed.Should().BeFalse();
		addedEmail.ConfirmedAt.Should().BeNull();
		addedEmail.IsPrimary.Should().BeFalse();
	}

	[Fact]
	public async Task AddEmail_EmailAlreadyBelongsToUser_ThrowsInvalidInputException()
	{
		const string email = "existing@example.com";
		var user = await CreateUser(email);

		var action = () => Mediator.Send(
			new AddEmailToUserCommand(
				user.Id,
				email,
				EmailType.Personal));

		var exception = await action.Should().ThrowAsync<InvalidInputException>();
		exception.Which.MessageKey.Should().Be("user.have.duplicate.email");
	}

	[Fact]
	public async Task AddEmail_EmailBelongsToAnotherUser_ThrowsConflictException()
	{
		const string email = "occupied@example.com";
		await CreateUser(email);
		var user = await CreateUser();

		var action = () => Mediator.Send(
			new AddEmailToUserCommand(
				user.Id,
				email,
				EmailType.Personal));

		await action.Should().ThrowAsync<UserEmailAlreadyInUseException>();
	}

	[Fact]
	public async Task AddEmail_EmailLimitReached_ThrowsInvalidInputException()
	{
		var options = Scope.ServiceProvider.GetRequiredService<IOptions<UserEmailOptions>>().Value;
		var builder = new MemberUserBuilder(Faker);

		for (var index = 0; index < options.MaxEmailCount; index++)
			builder.WithEmail($"email-{index}@example.com", isPrimary: index == 0);

		var user = await builder.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var action = () => Mediator.Send(
			new AddEmailToUserCommand(
				user.Id,
				"email-over-limit@example.com",
				EmailType.Personal));

		var exception = await action.Should().ThrowAsync<InvalidInputException>();
		exception.Which.MessageKey.Should().Be("user.max.email.count");
	}

	[Fact]
	public async Task AddEmail_UserDoesNotExist_ThrowsUserNotFoundException()
	{
		var userId = Guid.NewGuid();

		var action = () => Mediator.Send(
			new AddEmailToUserCommand(
				userId,
				"email@example.com",
				EmailType.Personal));

		await action.Should().ThrowAsync<UserNotFoundException>();
	}

	[Theory]
	[InlineData("invalid-email", EmailType.Personal)]
	[InlineData("valid@example.com", (EmailType)999)]
	public async Task AddEmail_InvalidInput_ThrowsValidationException(string email, EmailType emailType)
	{
		var user = await CreateUser();

		var action = () => Mediator.Send(
			new AddEmailToUserCommand(
				user.Id,
				email,
				emailType));

		await action.Should().ThrowAsync<ValidationException>();
	}

	private async Task<User> CreateUser(string? primaryEmail = null)
	{
		var builder = new MemberUserBuilder(Faker);

		if (primaryEmail is not null)
			builder.WithEmail(primaryEmail, isPrimary: true);

		var user = await builder.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();
		return user;
	}
}
