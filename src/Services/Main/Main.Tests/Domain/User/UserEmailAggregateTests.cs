using Exceptions;
using FluentAssertions;
using Main.Entities.DomainEvents.User;
using Main.Entities.Exceptions;
using Main.Enums;

namespace Tests.Domain.User;

public class UserEmailAggregateTests
{
	[Fact]
	public void AddEmail_ValidData_AddsEmail()
	{
		var user = CreateUser();

		user.AddEmail(
			"email@example.com",
			EmailType.Work,
			3);

		var email = user.Emails.Should().ContainSingle().Subject;
		email.Email.Value.Should().Be("email@example.com");
		email.EmailType.Should().Be(EmailType.Work);
		email.Confirmed.Should().BeFalse();
		email.IsPrimary.Should().BeFalse();
	}

	[Fact]
	public void AddEmail_DuplicateEmail_Throws()
	{
		var user = CreateUser();
		user.AddEmail(
			"email@example.com",
			EmailType.Personal,
			false,
			false);

		var action = () => user.AddEmail(
			"EMAIL@example.com",
			EmailType.Work,
			3);

		var exception = action.Should().Throw<InvalidInputException>().Which;
		exception.MessageKey.Should().Be("user.have.duplicate.email");
		user.Emails.Should().ContainSingle();
	}

	[Fact]
	public void AddEmail_EmailLimitReached_Throws()
	{
		var user = CreateUser();
		user.AddEmail(
			"first@example.com",
			EmailType.Personal,
			true,
			true);

		var action = () => user.AddEmail(
			"second@example.com",
			EmailType.Work,
			1);

		var exception = action.Should().Throw<InvalidInputException>().Which;
		exception.MessageKey.Should().Be("user.max.email.count");
		user.Emails.Should().ContainSingle();
	}

	[Fact]
	public void RemoveEmail_ExistingEmail_RemovesEmail()
	{
		var user = CreateUser();
		user.AddEmail(
			"first@example.com",
			EmailType.Personal,
			true,
			true);
		user.AddEmail(
			"second@example.com",
			EmailType.Work,
			false,
			false);

		user.RemoveEmail("second@example.com", 1);

		user.Emails.Should().ContainSingle();
		user.Emails.Single().Email.Value.Should().Be("first@example.com");
	}

	[Fact]
	public void RemoveEmail_EmailDoesNotExist_Throws()
	{
		var user = CreateUser();
		user.AddEmail(
			"email@example.com",
			EmailType.Personal,
			true,
			true);

		var action = () => user.RemoveEmail("missing@example.com", 0);

		action.Should().Throw<UserEmailNotFoundException>();
		user.Emails.Should().ContainSingle();
	}

	[Fact]
	public void RemoveEmail_PrimaryEmail_Throws()
	{
		var user = CreateUser();
		user.AddEmail(
			"primary@example.com",
			EmailType.Personal,
			true,
			true);
		user.AddEmail(
			"secondary@example.com",
			EmailType.Work,
			false,
			true);

		var action = () => user.RemoveEmail("primary@example.com", 1);

		var exception = action.Should().Throw<InvalidInputException>().Which;
		exception.MessageKey.Should().Be("user.email.primary.cannot.delete");
		user.Emails.Should().HaveCount(2);
		user.Emails.Single(x => x.IsPrimary).Email.Value.Should().Be("primary@example.com");
	}

	[Fact]
	public void RemoveEmail_MinimumEmailCountReached_Throws()
	{
		var user = CreateUser();
		user.AddEmail(
			"email@example.com",
			EmailType.Personal,
			false,
			true);

		var action = () => user.RemoveEmail("email@example.com", 1);

		var exception = action.Should().Throw<InvalidInputException>().Which;
		exception.MessageKey.Should().Be("user.min.email.count");
		user.Emails.Should().ContainSingle();
	}

	[Fact]
	public void MakeEmailPrimary_ConfirmedEmail_ReplacesCurrentPrimary()
	{
		var user = CreateUser();
		user.AddEmail(
			"current@example.com",
			EmailType.Personal,
			true,
			true);
		user.AddEmail(
			"new@example.com",
			EmailType.Work,
			false,
			true);

		user.MakeEmailPrimary("new@example.com");

		user.Emails.Should().ContainSingle(x => x.IsPrimary);
		user.Emails.Single(x => x.IsPrimary).Email.Value.Should().Be("new@example.com");
		user.Emails.Single(x => x.Email == "current@example.com").IsPrimary.Should().BeFalse();
	}

	[Fact]
	public void MakeEmailPrimary_UnconfirmedEmail_ThrowsAndKeepsCurrentPrimary()
	{
		var user = CreateUser();
		user.AddEmail(
			"current@example.com",
			EmailType.Personal,
			true,
			true);
		user.AddEmail(
			"unconfirmed@example.com",
			EmailType.Work,
			false,
			false);

		var action = () => user.MakeEmailPrimary("unconfirmed@example.com");

		var exception = action.Should().Throw<InvalidInputException>().Which;
		exception.MessageKey.Should().Be("user.email.primary.must.be.confirmed");
		user.Emails.Single(x => x.IsPrimary).Email.Value.Should().Be("current@example.com");
	}

	[Fact]
	public void MakeEmailPrimary_EmailDoesNotExist_Throws()
	{
		var user = CreateUser();

		var action = () => user.MakeEmailPrimary("missing@example.com");

		action.Should().Throw<UserEmailNotFoundException>();
	}

	[Fact]
	public void EmailLifecycle_PublishesUserUpdatedEvent()
	{
		var user = CreateUser();
		user.AddEmail(
			"email@example.com",
			EmailType.Personal,
			false,
			false);
		var email = user.Emails.Single();

		email.OnCreated();

		var @event = email
			.FlushDomainEvents()
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.BeOfType<UserUpdatedDomainEvent>()
			.Which;
		@event.UserId.Should().Be(user.Id);
	}

	private static Main.Entities.User.User CreateUser()
	{
		return Main.Entities.User.User.Create("test-user", "password-hash");
	}
}
