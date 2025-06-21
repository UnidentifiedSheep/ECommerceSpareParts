using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles;

public class LinkageCrossArticleIdException(int articleId) : BadRequestException($"Кросс артикул не может быть равен артикулу. Id = {articleId}")
{
    
}