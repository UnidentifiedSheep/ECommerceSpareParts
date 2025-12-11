using Exceptions.Base;

namespace Exceptions.Exceptions.ArticleCharacteristics;

public class ArticleCharacteristicsNotFoundException : NotFoundException
{
    public ArticleCharacteristicsNotFoundException(int id) : base("Не удалось найти характеристику", new { Id = id })
    {
    }
}