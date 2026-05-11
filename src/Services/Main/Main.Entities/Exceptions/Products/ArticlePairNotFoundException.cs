using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Products;

public class ArticlePairNotFoundException : NotFoundException, ILocalizableException
{
    public ArticlePairNotFoundException(int articleId) : base(null, new { ArticleId = articleId })
    {
    }

    public string MessageKey => "article.pair.not.found";
    public object[]? Arguments => null;
}