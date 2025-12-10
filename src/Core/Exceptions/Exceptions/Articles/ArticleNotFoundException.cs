using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Articles;

public class ArticleNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, 123)]
    public ArticleNotFoundException(object id) : base("Не удалось найти артикул.", new { Id = id })
    {
    }

    [ExampleExceptionValues(true, 123, 456, 7890)]
    public ArticleNotFoundException(IEnumerable<int> ids) : base("Не удалось найти артикулы.", new { Ids = ids })
    {
    }

    [ExampleExceptionValues]
    public ArticleNotFoundException() : base("Не удалось найти какой-то артикул")
    {
    }
}