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
            maxEmailCount: 3);

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
            maxEmailCount: 3);

        var exception = action.Should()
            .Throw<InvalidInputException>()
            .Which;
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
            maxEmailCount: 1);

        var exception = action.Should()
            .Throw<InvalidInputException>()
            .Which;
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

        user.RemoveEmail(
            "second@example.com",
            minEmailCount: 1);

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

        var action = () => user.RemoveEmail(
            "missing@example.com",
            minEmailCount: 0);

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

        var action = () => user.RemoveEmail(
            "primary@example.com",
            minEmailCount: 1);

        var exception = action.Should()
            .Throw<InvalidInputException>()
            .Which;
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

        var action = () => user.RemoveEmail(
            "email@example.com",
            minEmailCount: 1);

        var exception = action.Should()
            .Throw<InvalidInputException>()
            .Which;
        exception.MessageKey.Should().Be("user.min.email.count");
        user.Emails.Should().ContainSingle();
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

        var @event = email.FlushDomainEvents()
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
        return Main.Entities.User.User.Create(
            "test-user",
            "password-hash");
    }
}
