using MediatR;

namespace Main.Application.Notifications;

public record CurrencyRatesUpdatedNotification() : INotification;