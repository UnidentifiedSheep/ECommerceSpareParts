namespace Core.RabbitMq.Contracts.Currency.DeleteCurrency;

public record CurrencyCheckRequestedEvent(int CurrencyId, Guid SagaId);