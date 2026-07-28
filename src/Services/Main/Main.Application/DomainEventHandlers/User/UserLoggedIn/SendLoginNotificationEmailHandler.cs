using Abstractions.Interfaces.Mail;
using Application.Common.Abstractions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Localization.Abstractions.Interfaces;
using Mailing.Core;
using Mailing.Core.Models;
using Main.Application.Interfaces.Services;
using Main.Entities.DomainEvents.User;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.DomainEventHandlers.User.UserLoggedIn;

public class SendLoginNotificationEmailHandler(
    IReadRepository<UserEmail, string> emailRepository,
    IMailingService mailingService,
    IEmailMessageRenderer emailRenderer,
    IScopedStringLocalizer localizer,
    ILogger<SendLoginNotificationEmailHandler> logger)
    : BatchableDomainEventHandler<UserLoggedInDomainEvent>
{
    public override async Task Handle(
        Batch<UserLoggedInDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var events = notification.Items;
        if (events.Count == 0) return;

        var userIds = events
            .Select(x => x.UserId)
            .Distinct()
            .ToList();

        var primaryEmails = await emailRepository.Query
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId) && x.IsPrimary)
            .Select(x => new
            {
                x.UserId,
                Email = x.Email.Value
            })
            .ToDictionaryAsync(
                x => x.UserId,
                x => x.Email,
                cancellationToken);

        var emails = new List<IEmailMessage>(events.Count);

        foreach (var @event in events)
        {
            if (!primaryEmails.TryGetValue(@event.UserId, out var email))
            {
                logger.LogInformation(
                    "Login notification email skipped because primary email was not found. UserId: {UserId}",
                    @event.UserId);
                continue;
            }

            emails.Add(
                await emailRenderer.RenderAsync(
                    new LoginNotificationData(
                        localizer,
                        @event.OccurredAtUtc,
                        @event.IpAddress,
                        @event.UserAgent,
                        email),
                    cancellationToken));
        }

        await mailingService.QueueToOutbox(
            emails,
            cancellationToken);
    }
}
