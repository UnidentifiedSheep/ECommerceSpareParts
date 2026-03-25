using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Sales;

public class SaleContentNotFoundException : BadRequestException, ILocalizableException
{
    public string MessageKey => "sale.content.not.found";
    public object[]? Arguments => null;
    public SaleContentNotFoundException(int id) : base(null, new { Id = id })
    {
    }
}