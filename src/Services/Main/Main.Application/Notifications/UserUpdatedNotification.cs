using MediatR;

namespace Main.Application.Notifications;

public record UserUpdatedNotification(Guid UserId) : INotification;