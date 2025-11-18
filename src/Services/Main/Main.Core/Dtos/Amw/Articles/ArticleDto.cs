namespace Main.Core.Dtos.Amw.Articles;

public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string ArticleNumber { get; set; } = null!;
    public int ProducerId { get; set; }
    public string ProducerName { get; set; } = null!;
    public int CurrentStock { get; set; }
    public string? Indicator { get; set; }
}