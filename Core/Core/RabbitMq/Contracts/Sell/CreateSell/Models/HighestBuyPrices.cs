namespace Core.RabbitMq.Contracts.Sell.CreateSell.Models;

public class HighestBuyPrices
{
    public int ArticleId { get; set; }
    public decimal BuyPrice { get; set; }
    public int CurrencyId { get; set; }
}