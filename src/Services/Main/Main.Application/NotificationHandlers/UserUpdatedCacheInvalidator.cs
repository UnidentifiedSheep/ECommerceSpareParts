using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.RelatedData;
using Main.Application.Notifications;
using Main.Entities;
using Main.Entities.User;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class UserUpdatedCacheInvalidator(
    IRelatedDataRepository<User> relatedDataBase,
    ICache cache) : INotificationHandler<UserUpdatedNotification>
{
    public async Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var relatedDataKeys = await relatedDataBase.GetRelatedDataKeys(notification.UserId.ToString());
        await cache.DeleteAsync(relatedDataKeys);
    }
}