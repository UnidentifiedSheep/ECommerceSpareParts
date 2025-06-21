namespace Core.RabbitMq.Contracts.Sell.CreateSell;

public record SellCreationRequestedEvent(string WhoCreatedUserId, string SellingToWhom, int CurrencyId, Guid SagaId, 
    DateTime SellDateTime, string? Comment, string SellContent, string StorageName, bool TakeFromOtherStorages);