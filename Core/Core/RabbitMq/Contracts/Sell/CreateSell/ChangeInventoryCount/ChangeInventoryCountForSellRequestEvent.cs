using Core.RabbitMq.Contracts.Sell.CreateSell.Models;

namespace Core.RabbitMq.Contracts.Sell.CreateSell.ChangeInventoryCount;

public record ChangeInventoryCountForSellRequestEvent(string StorageName, bool TakeFromOtherStorages, int CurrencyId, IEnumerable<SellContent> Articles, Guid SagaId);