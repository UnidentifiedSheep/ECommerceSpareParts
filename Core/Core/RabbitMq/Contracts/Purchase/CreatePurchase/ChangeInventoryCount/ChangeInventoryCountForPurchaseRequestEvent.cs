using Core.RabbitMq.Contracts.Purchase.CreatePurchase.Models;

namespace Core.RabbitMq.Contracts.Purchase.CreatePurchase.ChangeInventoryCount;

public record ChangeInventoryCountForPurchaseRequestEvent(string StorageName, int CurrencyId, List<ArticleModel> Articles, Guid SagaId);