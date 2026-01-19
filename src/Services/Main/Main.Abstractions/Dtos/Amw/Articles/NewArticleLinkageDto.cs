using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.Articles;

public class NewArticleLinkageDto
{
    public int ArticleId { get; set; }
    public int CrossArticleId { get; set; }
    public ArticleLinkageType LinkageType { get; set; }
}