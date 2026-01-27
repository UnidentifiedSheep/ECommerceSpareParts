using MediatR;

namespace Main.Application.Notifications;

public record ArticleSizeUpdatedNotification(int ArticleId) : INotification;