using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ArticleNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey { get; }
    public object[]? Arguments => null;
    public ArticleNotFoundException(int id) : base(null, new { Id = id })
    {
        MessageKey = "article.not.found";
    }

    public ArticleNotFoundException(IEnumerable<int> ids) : base(null, new { Ids = ids })
    {
        MessageKey = "articles.not.found";
    }

}