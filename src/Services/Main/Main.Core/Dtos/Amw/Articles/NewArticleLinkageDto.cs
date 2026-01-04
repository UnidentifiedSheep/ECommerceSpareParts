using Main.Core.Enums;

namespace Main.Core.Dtos.Amw.Articles;

public class NewArticleLinkageDto
{
    public int ArticleId { get; set; }
    public int CrossArticleId { get; set; }
    public ArticleLinkageType LinkageType { get; set; }
}