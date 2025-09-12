using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Articles;

public class LinkageCrossArticleIdException(int id) : BadRequestException("Кросс артикул не может быть равен артикулу.", new { Id = id })
{
    
}