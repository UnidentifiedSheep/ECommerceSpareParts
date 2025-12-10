using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class ArticleDoesntMatchContentException : BadRequestException
{
    [ExampleExceptionValues(false, 123)]
    public ArticleDoesntMatchContentException(int id) : base("Артикул не соответствует позиции в продаже", new { Id = id })
    {
    }
}