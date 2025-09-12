using MediatR;

namespace Application.Events;

public record ArticleUpdatedEvent(int ArticleId) : INotification;