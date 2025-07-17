using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles;

public class LinkageCrossArticleIdException(int id) : BadRequestException("Кросс артикул не может быть равен артикулу.", new { Id = id })
{
    
}