using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ProductNotFoundException : NotFoundException, ILocalizableException
{
    public ProductNotFoundException(int id) : base(null, new { Id = id })
    {
        MessageKey = "article.not.found";
    }

    public ProductNotFoundException(IEnumerable<int> ids) : base(null, new { Ids = ids })
    {
        MessageKey = "articles.not.found";
    }

    public string MessageKey { get; }
    public object[]? Arguments => null;
}