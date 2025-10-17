using MediatR;

namespace Main.Application.Notifications;

public record CurrencyCreatedNotification(int Id) : INotification;