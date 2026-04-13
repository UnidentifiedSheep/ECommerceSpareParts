using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ProductContentNotFoundException : NotFoundException, ILocalizableException
{
    public ProductContentNotFoundException(int articleId, int insideArticleId) : base(
        null,
        new { MainArticleId = articleId, InsideArticleId = insideArticleId })
    {
    }

    public string MessageKey => "article.content.not.found";
    public object[]? Arguments => null;
}