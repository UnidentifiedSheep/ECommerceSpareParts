using MonoliteUnicorn.Enums;

namespace MonoliteUnicorn.Dtos.Amw.Articles;

public class NewArticleLinkageDto
{
    public int ArticleId { get; set; }
    public int CrossArticleId { get; set; }
    public ArticleLinkageTypes LinkageType { get; set; }
}