using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Products;

public class ProductCharacteristicsNotFoundException : NotFoundException, ILocalizableException
{
    public ProductCharacteristicsNotFoundException(int id, string name) 
        : base(null, new { Id = id, Name = name })
    {
    }

    public string MessageKey => "article.characteristics.not.found";
    public object[]? Arguments => null;
}