using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ProductSizesNotFoundException : NotFoundException, ILocalizableException
{
    public ProductSizesNotFoundException(int articleId) : base(null, new { ArticleId = articleId })
    {
    }

    public string MessageKey => "article.sizes.not.found";
    public object[]? Arguments => null;
}