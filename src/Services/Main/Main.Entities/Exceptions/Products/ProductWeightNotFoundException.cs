using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Products;

public class ProductWeightNotFoundException : NotFoundException, ILocalizableException
{
    public ProductWeightNotFoundException(int articleId) : base(null, new { ArticleId = articleId })
    {
    }

    public string MessageKey => "article.weight.not.found";
    public object[]? Arguments => null;
}