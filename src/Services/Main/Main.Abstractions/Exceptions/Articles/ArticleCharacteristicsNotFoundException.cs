using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Articles;

public class ArticleCharacteristicsNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "article.characteristics.not.found";
    public object[]? Arguments => null;
    public ArticleCharacteristicsNotFoundException(int id) : base(null, new { Id = id })
    {
    }

}