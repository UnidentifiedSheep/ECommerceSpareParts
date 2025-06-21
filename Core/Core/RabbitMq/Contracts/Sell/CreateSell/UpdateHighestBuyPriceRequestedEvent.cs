namespace Core.RabbitMq.Contracts.Sell.CreateSell;

public record UpdateHighestBuyPriceRequestedEvent(IEnumerable<int> Articles);