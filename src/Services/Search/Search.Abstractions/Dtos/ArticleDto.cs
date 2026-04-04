namespace Search.Abstractions.Dtos;

public partial class ArticleDto
{
    public int Id { get; set; }

    public string ArticleNumber { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int ProducerId { get; set; }

    public string ProducerName { get; set; } = null!;

    public long Popularity { get; set; }
}