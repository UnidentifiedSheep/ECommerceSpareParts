using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ArticlePairNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "article.pair.not.found";
    public object[]? Arguments => null;
    public ArticlePairNotFoundException(int articleId) : base(null, new { ArticleId = articleId })
    {
    }

}