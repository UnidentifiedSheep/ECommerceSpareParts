namespace Analytics.Core.Entities;

public partial class Article
{
    public int ArticleId { get; set; }

    public string StorageName { get; set; } = null!;

    public int TotalCount { get; set; }
}
