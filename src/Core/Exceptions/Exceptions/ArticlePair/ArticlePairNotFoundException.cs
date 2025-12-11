using Exceptions.Base;

namespace Exceptions.Exceptions.ArticlePair;

public class ArticlePairNotFoundException : NotFoundException
{
    public ArticlePairNotFoundException(int articleId) : base("Не удалось найти пару артикула", new { ArticleId = articleId })
    {
    }
}