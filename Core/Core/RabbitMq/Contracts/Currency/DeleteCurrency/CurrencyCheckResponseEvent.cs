namespace Core.RabbitMq.Contracts.Currency.DeleteCurrency;

public record CurrencyCheckResponseEvent(int CurrencyId, bool CanDelete, Guid SagaId);