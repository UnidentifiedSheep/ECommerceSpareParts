using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Users.RemoveEmailFromUser;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Users;

public class RemoveEmailFromUserTests(
    CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task RemoveEmail_SecondaryEmail_RemovesEmail()
    {
        var user = await CreateUser();

        await Mediator.Send(
            new RemoveEmailFromUserCommand(
                user.Id,
                "secondary@example.com"));

        var emails = await Context.UserEmails
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .ToListAsync();

        emails.Should().ContainSingle();
        emails.Single().Email.Value.Should().Be("primary@example.com");
        emails.Single().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveEmail_PrimaryEmail_ThrowsAndKeepsEmail()
    {
        var user = await CreateUser();

        var action = () => Mediator.Send(
            new RemoveEmailFromUserCommand(
                user.Id,
                "primary@example.com"));

        var exception = await action.Should()
            .ThrowAsync<InvalidInputException>();
        exception.Which.MessageKey.Should().Be("user.email.primary.cannot.delete");

        Context.ChangeTracker.Clear();
        var emails = await Context.UserEmails
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .ToListAsync();
        emails.Should().HaveCount(2);
        emails.Single(x => x.IsPrimary).Email.Value.Should().Be("primary@example.com");
    }

    private async Task<Main.Entities.User.User> CreateUser()
    {
        var builder = new MemberUserBuilder(Faker);
        builder
            .WithEmail(
                "primary@example.com",
                isPrimary: true)
            .WithEmail("secondary@example.com");

        var user = await builder.BuildAndAddToDb(Context);
        Context.ChangeTracker.Clear();
        return user;
    }
}
