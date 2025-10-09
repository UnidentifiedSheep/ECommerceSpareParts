using Exceptions.Base;

namespace Exceptions.Exceptions.ArticlePair;

public class ArticlePairNotFoundException(int articleId)
    : NotFoundException("Не удалось найти пару артикула", new { ArticleId = articleId })
{
}