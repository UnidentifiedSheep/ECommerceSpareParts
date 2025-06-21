namespace Core.RabbitMq.Contracts.Articles.Models;

public class ArticleModel
{
    public int Id { get; set; }
    public int ProducerId { get; set; }
    public string ProducerName { get; set; }
    public string ArticleNumber { get; set; }
    public string NormalizedArticleNumber { get; set; }
    public string ArticleTitle { get; set; }
}