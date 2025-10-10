namespace Main.Core.Dtos.Anonymous.Articles;

public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string ArticleNumber { get; set; } = null!;
    public string ProducerName { get; set; } = null!;
    public int CurrentStock { get; set; }
}