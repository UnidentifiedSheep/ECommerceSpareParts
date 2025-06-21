using Core.RabbitMq.Contracts.Purchase.CreatePurchase.Models;

namespace Core.RabbitMq.Contracts.Purchase.CreatePurchase.CreatePurchase;

public record CreatePurchaseRequestEvent(string WhoCreateUserId, string SupplierId, int CurrencyId, string? Comment, DateTime PurchaseDate, DateTime CreatedDate, List<ArticleModel> Articles, Guid SagaId);
