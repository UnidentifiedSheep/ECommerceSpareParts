using MediatR;

namespace Main.Application.Events;

public record ArticleUpdatedEvent(int ArticleId) : INotification;