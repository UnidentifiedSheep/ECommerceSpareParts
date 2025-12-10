using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Articles;

public class ArticleContentNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, 123, 456)]
    public ArticleContentNotFoundException(int articleId, int insideArticleId) : base(
        "Содержание артикула не было найдено.",
        new { MainArticleId = articleId, InsideArticleId = insideArticleId })
    {
    }
}