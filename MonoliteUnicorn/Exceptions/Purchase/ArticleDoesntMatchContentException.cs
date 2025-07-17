using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Purchase;

public class ArticleDoesntMatchContentException(int id) : BadRequestException($"Артикул не соответствует позиции в продаже", new { Id = id })
{
    
}