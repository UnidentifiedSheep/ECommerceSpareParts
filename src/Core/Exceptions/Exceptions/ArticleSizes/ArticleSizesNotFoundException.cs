using Exceptions.Base;

namespace Exceptions.Exceptions.ArticleSizes;

public class ArticleSizesNotFoundException : NotFoundException
{
    public ArticleSizesNotFoundException(int articleId) : base("Не удалось найти размеров артикула.", new { ArticleId = articleId })
    {
    }
}