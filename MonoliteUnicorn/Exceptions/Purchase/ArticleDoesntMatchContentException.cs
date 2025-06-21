using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Purchase;

public class ArticleDoesntMatchContentException(int articleId) : BadRequestException($"Артикул '{articleId}' не соответствует позиции в продаже")
{
    
}