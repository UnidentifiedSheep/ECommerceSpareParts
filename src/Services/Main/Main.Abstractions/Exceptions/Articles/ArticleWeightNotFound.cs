using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ArticleWeightNotFound : NotFoundException, ILocalizableException
{
    public string MessageKey => "article.weight.not.found";
    public object[]? Arguments => null;
    public ArticleWeightNotFound(int articleId) : base(null, new { ArticleId = articleId })
    {
    }

}