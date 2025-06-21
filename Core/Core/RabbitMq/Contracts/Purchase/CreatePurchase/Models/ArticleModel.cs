namespace Core.RabbitMq.Contracts.Purchase.CreatePurchase.Models;

public class ArticleModel
{
    public int ArticleId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    public string? Comment {get; set;}
}