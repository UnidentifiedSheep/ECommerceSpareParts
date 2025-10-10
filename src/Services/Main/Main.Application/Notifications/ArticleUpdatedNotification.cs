using MediatR;

namespace Main.Application.Notifications;

public record ArticleUpdatedNotification(int ArticleId) : INotification;