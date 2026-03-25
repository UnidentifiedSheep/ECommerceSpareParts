using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Purchase;

public class ArticleDoesntMatchContentException : BadRequestException, ILocalizableException
{
    public string MessageKey => "content.article.doesnt.match.purchase.position";
    public object[]? Arguments => null;
    public ArticleDoesntMatchContentException(int id) : base(null, new { Id = id })
    {
    }
}