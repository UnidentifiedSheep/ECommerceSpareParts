namespace MonoliteUnicorn.Dtos.Amw.Articles;

public class NewArticleDto
{
    public string ArticleNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int ProducerId { get; set; }
    public string? Description { get; set; }
    public bool IsOe { get; set; } = false;
    public int? PackingUnit { get; set; }
    public string? Indicator { get; set; }
    public int? CategoryId { get; set; }
}