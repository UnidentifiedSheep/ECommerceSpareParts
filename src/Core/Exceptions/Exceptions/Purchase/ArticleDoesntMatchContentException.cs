using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class ArticleDoesntMatchContentException : BadRequestException
{
    public ArticleDoesntMatchContentException(int id) : base("Артикул не соответствует позиции в продаже", new { Id = id })
    {
    }
}