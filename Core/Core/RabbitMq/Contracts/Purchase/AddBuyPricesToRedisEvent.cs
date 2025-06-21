using Core.RabbitMq.Contracts.Purchase.CreatePurchase.Models;

namespace Core.RabbitMq.Contracts.Purchase;

public record AddBuyPricesToRedisEvent(int CurrencyId, IEnumerable<ArticleModel> Articles, DateTime PurchasedOn, string PurchaseId);