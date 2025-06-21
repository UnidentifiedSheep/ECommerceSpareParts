using Core.RabbitMq.Contracts.Sell.CreateSell.Models;

namespace Core.RabbitMq.Contracts.Sell.CreateSell;

public record UpdateHighestBuyPriceResultEvent(List<HighestBuyPrices> HighestBuyPricesList);