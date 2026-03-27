using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Sales;

public class SaleNotFoundException : NotFoundException, ILocalizableException
{
    public SaleNotFoundException(string id) : base(null, new { Id = id })
    {
    }

    public string MessageKey => "sale.not.found";
    public object[]? Arguments => null;
}