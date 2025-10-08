using MediatR;

namespace Main.Application.Events;

public record ArticlePricesUpdatedEvent(IEnumerable<int> ArticleIds) : INotification;