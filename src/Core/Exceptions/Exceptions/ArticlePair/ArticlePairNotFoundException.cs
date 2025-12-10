using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.ArticlePair;

public class ArticlePairNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, 123)]
    public ArticlePairNotFoundException(int articleId) : base("Не удалось найти пару артикула", new { ArticleId = articleId })
    {
    }
}