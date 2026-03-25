using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Sales;

public class SaleNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "sale.not.found";
    public object[]? Arguments => null;
    public SaleNotFoundException(string id) : base(null, new { Id = id }) { }
}