using MediatR;

namespace Application.Events;

public record ArticlePricesUpdatedEvent(IEnumerable<int> ArticleIds) : INotification;