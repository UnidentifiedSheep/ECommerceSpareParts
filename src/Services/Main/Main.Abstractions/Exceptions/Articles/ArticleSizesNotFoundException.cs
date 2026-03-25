using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ArticleSizesNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "article.sizes.not.found";
    public object[]? Arguments => null;
    public ArticleSizesNotFoundException(int articleId) : base(null, new { ArticleId = articleId })
    {
    }

}