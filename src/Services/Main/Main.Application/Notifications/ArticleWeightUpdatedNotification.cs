using MediatR;

namespace Main.Application.Notifications;

public record ArticleWeightUpdatedNotification(int ArticleId) : INotification;