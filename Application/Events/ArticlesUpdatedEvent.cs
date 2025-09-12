using MediatR;

namespace Application.Events;

public record ArticlesUpdatedEvent(IEnumerable<int> ArticleIds) : INotification;