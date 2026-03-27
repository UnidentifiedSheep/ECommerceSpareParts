using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ArticleContentNotFoundException : NotFoundException, ILocalizableException
{
    public ArticleContentNotFoundException(int articleId, int insideArticleId) : base(
        null,
        new { MainArticleId = articleId, InsideArticleId = insideArticleId })
    {
    }

    public string MessageKey => "article.content.not.found";
    public object[]? Arguments => null;
}