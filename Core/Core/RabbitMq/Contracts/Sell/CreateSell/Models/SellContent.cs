namespace Core.RabbitMq.Contracts.Sell.CreateSell.Models;

public class SellContent
{
    public int ArticleId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    public decimal TotalSum { get; set; }
    public string? Comment { get; set; }
    public decimal Discount { get; set; } 
}