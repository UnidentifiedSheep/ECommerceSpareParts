namespace Core.RabbitMq.Contracts.Currency.DeleteCurrency;

public record CurrencyDeleteRequestedEvent(int CurrencyId, Guid SagaId);