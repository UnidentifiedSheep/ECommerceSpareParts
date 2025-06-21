namespace Core.RabbitMq.Contracts.Currency.DeleteCurrency;

public record CurrencyDeleted(int CurrencyId, Guid SagaId);