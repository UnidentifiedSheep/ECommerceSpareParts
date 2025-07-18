using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.Dtos.Anonymous.Articles;

public class ContentArticleDto
{
    public int Quantity { get; set; }
    public ArticleDto Article { get; set; } = null!;
}