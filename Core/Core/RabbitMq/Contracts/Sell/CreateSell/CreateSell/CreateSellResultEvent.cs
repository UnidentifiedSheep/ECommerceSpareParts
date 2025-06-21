namespace Core.RabbitMq.Contracts.Sell.CreateSell.CreateSell;

public record CreateSellResultEvent(bool IsSuccess, string? SellId, Guid SagaId);