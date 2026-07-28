using Abstractions.Interfaces.Mail;
using Abstractions.Models.Mail;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using FluentAssertions;
using Localization.Abstractions.Interfaces;
using Mailing.Core;
using Mailing.Core.Models;
using Main.Application.DomainEventHandlers.User;
using Main.Application.Interfaces.Services;
using Main.Entities.DomainEvents.User;
using Main.Entities.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.DomainEventHandlers.User;

public class SendLoginNotificationEmailHandlerTests(
    CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Handle_LoadsPrimaryEmailsAndQueuesMessagesAsBatch()
    {
        var firstUser = await new MemberUserBuilder(Faker)
            .WithUserName("first-user")
            .WithEmail("first-primary@example.com", isPrimary: true)
            .WithEmail("first-secondary@example.com")
            .BuildAndAddToDb(Context);
        var secondUser = await new MemberUserBuilder(Faker)
            .WithUserName("second-user")
            .WithEmail("second-primary@example.com", isPrimary: true)
            .WithEmail("second-secondary@example.com")
            .BuildAndAddToDb(Context);

        Context.ChangeTracker.Clear();

        var renderer = new Mock<IEmailMessageRenderer>();
        renderer
            .Setup(x => x.RenderAsync(
                It.IsAny<LoginNotificationData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                LoginNotificationData data,
                CancellationToken _) => new EmailMessage(
                data.Subject,
                data.To,
                "body"));

        IReadOnlyList<IEmailMessage>? queuedEmails = null;
        var mailingService = new Mock<IMailingService>();
        mailingService
            .Setup(x => x.QueueToOutbox(
                It.IsAny<IEnumerable<IEmailMessage>>(),
                It.IsAny<CancellationToken>()))
            .Callback((
                IEnumerable<IEmailMessage> emails,
                CancellationToken _) => queuedEmails = emails.ToList())
            .Returns(Task.CompletedTask);

        var handler = new SendLoginNotificationEmailHandler(
            Scope.ServiceProvider.GetRequiredService<IReadRepository<UserEmail, string>>(),
            mailingService.Object,
            renderer.Object,
            Scope.ServiceProvider.GetRequiredService<IScopedStringLocalizer>(),
            NullLogger<SendLoginNotificationEmailHandler>.Instance);

        await handler.Handle(
            new Batch<UserLoggedInDomainEvent>(
            [
                new UserLoggedInDomainEvent(
                    firstUser.Id,
                    DateTime.UtcNow,
                    "192.0.2.1",
                    "Chrome Windows"),
                new UserLoggedInDomainEvent(
                    secondUser.Id,
                    DateTime.UtcNow,
                    "192.0.2.2",
                    "Firefox Linux")
            ]),
            CancellationToken.None);

        queuedEmails.Should().NotBeNull();
        queuedEmails!.Select(x => x.To).Should().BeEquivalentTo(
            "first-primary@example.com",
            "second-primary@example.com");
        mailingService.Verify(
            x => x.QueueToOutbox(
                It.IsAny<IEnumerable<IEmailMessage>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
