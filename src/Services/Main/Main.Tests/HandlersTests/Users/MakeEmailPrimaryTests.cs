using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Users.MakeEmailPrimary;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Users;

public class MakeEmailPrimaryTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task MakeEmailPrimary_ConfirmedEmail_ReplacesCurrentPrimary()
	{
		var user = await CreateUser();

		await Mediator.Send(new MakeEmailPrimaryCommand(user.Id, "new-primary@example.com"));

		var emails = await Context.UserEmails.AsNoTracking().Where(x => x.UserId == user.Id).ToListAsync();
		emails.Should().ContainSingle(x => x.IsPrimary);
		emails.Single(x => x.IsPrimary).Email.Value.Should().Be("new-primary@example.com");
	}

	[Fact]
	public async Task MakeEmailPrimary_UnconfirmedEmail_Throws()
	{
		var user = await CreateUser(false);

		var action = () => Mediator.Send(new MakeEmailPrimaryCommand(user.Id, "new-primary@example.com"));

		var exception = await action.Should().ThrowAsync<InvalidInputException>();
		exception.Which.MessageKey.Should().Be("user.email.primary.must.be.confirmed");

		Context.ChangeTracker.Clear();
		var primaryEmail = await Context
			.UserEmails
			.AsNoTracking()
			.SingleAsync(x => x.UserId == user.Id && x.IsPrimary);
		primaryEmail.Email.Value.Should().Be("current-primary@example.com");
	}

	[Fact]
	public async Task MakeEmailPrimary_EmailDoesNotExist_Throws()
	{
		var user = await CreateUser();

		var action = () => Mediator.Send(new MakeEmailPrimaryCommand(user.Id, "missing@example.com"));

		await action.Should().ThrowAsync<UserEmailNotFoundException>();
	}

	[Fact]
	public async Task MakeEmailPrimary_UserDoesNotExist_Throws()
	{
		var action = () => Mediator.Send(new MakeEmailPrimaryCommand(Guid.NewGuid(), "email@example.com"));

		await action.Should().ThrowAsync<UserNotFoundException>();
	}

	private async Task<User> CreateUser(bool newEmailConfirmed = true)
	{
		var user = await new MemberUserBuilder(Faker)
			.WithEmail("current-primary@example.com", isPrimary: true)
			.WithEmail("new-primary@example.com", isConfirmed: newEmailConfirmed)
			.BuildAndAddToDb(Context);

		Context.ChangeTracker.Clear();
		return user;
	}
}
