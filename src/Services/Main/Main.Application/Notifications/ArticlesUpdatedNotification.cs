using MediatR;

namespace Main.Application.Notifications;

public record ArticlesUpdatedNotification(IEnumerable<int> ArticleIds) : INotification;