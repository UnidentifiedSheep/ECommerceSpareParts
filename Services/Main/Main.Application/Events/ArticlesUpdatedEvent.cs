using MediatR;

namespace Main.Application.Events;

public record ArticlesUpdatedEvent(IEnumerable<int> ArticleIds) : INotification;