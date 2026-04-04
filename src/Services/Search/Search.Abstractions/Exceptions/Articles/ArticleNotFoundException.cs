using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Search.Abstractions.Exceptions.Articles;

public class ArticleNotFoundException : NotFoundException, ILocalizableException
{
    public ArticleNotFoundException(int id) : base(null, new { Id = id })
    {
    }

    public ArticleNotFoundException(IEnumerable<int> ids) : base(null, new { Ids = ids })
    {
    }

    public string MessageKey => "article.not.found";
    public object[]? Arguments => null;
}