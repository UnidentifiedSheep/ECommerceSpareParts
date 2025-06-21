using Core.RabbitMq.Contracts.Sell.CreateSell.Models;

namespace Core.RabbitMq.Contracts.Sell.CreateSell.CreateSell;

public record CreateSellRequestEvent(string WhoCreatedUserId, string BuyerId, string? Comment, DateTime SaleDateTime, 
    DateTime CreationDateTime, int CurrencyId, Guid SagaId, IEnumerable<SellContent> SellContent);