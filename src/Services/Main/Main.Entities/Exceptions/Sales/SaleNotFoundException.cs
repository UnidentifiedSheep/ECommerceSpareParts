using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Sales;

public class SaleNotFoundException : NotFoundException, ILocalizableException
{
    public SaleNotFoundException(Guid id) : base(null, new { Id = id })
    {
    }

    public string MessageKey => "sale.not.found";
    public object[]? Arguments => null;
}