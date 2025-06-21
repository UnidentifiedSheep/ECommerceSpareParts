namespace Core.RabbitMq.Contracts.Purchase;

public record PurchaseMadeEvent(IEnumerable<(int ArticleId, double Price)> ArticleIdPrice, int CurrencyId, string OfferedFrom, DateTime PurchasedDateTime);