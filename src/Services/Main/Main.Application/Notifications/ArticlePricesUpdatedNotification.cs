using MediatR;

namespace Main.Application.Notifications;

public record ArticlePricesUpdatedNotification(IEnumerable<int> ArticleIds) : INotification;