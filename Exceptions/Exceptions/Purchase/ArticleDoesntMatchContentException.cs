using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Purchase;

public class ArticleDoesntMatchContentException(int id) : BadRequestException($"Артикул не соответствует позиции в продаже", new { Id = id })
{
    
}