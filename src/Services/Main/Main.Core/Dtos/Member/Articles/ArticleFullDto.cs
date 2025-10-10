namespace Main.Core.Dtos.Member.Articles;

public class ArticleFullDto
{
    public int Id { get; set; }
    public string ArticleNumber { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ProducerId { get; set; }
    public string ProducerName { get; set; } = null!;
    public List<string> Images { get; set; } = [];
    public int CurrentStock { get; set; }
}