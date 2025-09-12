using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class ArticleDoesntMatchContentException(int id)
    : BadRequestException("Артикул не соответствует позиции в продаже", new { Id = id })
{
}