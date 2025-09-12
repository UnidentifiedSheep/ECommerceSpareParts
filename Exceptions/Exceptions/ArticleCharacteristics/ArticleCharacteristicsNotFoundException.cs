using Exceptions.Base;

namespace Exceptions.Exceptions.ArticleCharacteristics;

public class ArticleCharacteristicsNotFoundException(int id) : NotFoundException("Не удалось найти характеристику", new { Id = id})
{
    
}