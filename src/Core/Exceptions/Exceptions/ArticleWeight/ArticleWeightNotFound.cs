using Exceptions.Base;

namespace Exceptions.Exceptions.ArticleWeight;

public class ArticleWeightNotFound : NotFoundException
{
    public ArticleWeightNotFound(int articleId) : base("Не удалось найти вес для артикула", new { ArticleId = articleId })
    {
    }
}