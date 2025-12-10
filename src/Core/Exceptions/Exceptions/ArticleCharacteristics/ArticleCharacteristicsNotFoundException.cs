using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.ArticleCharacteristics;

public class ArticleCharacteristicsNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, 123)]
    public ArticleCharacteristicsNotFoundException(int id) : base("Не удалось найти характеристику", new { Id = id })
    {
    }
}