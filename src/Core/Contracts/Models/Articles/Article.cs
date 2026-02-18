namespace Contracts.Models.Articles;

public record Article
{
    public int Id { get; init; }
    public string ArticleNumber { get; init; } = null!;
    public string NormalizedArticleNumber { get; init; } = null!;
    public string ArticleName { get; init; } = null!;
    public string? Description { get; init; }
    public int? PackingUnit { get; init; }
    public int ProducerId { get; init; }
    public string ProducerName { get; init; } = null!;
    public bool IsOe { get; init; }
    public int TotalCount { get; init; }
    public string? Indicator { get; init; }
    public int? CategoryId { get; init; }
    public long Popularity { get; init; }
}