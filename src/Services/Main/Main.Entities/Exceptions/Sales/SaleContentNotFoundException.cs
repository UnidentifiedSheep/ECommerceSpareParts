using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Sales;

public class SaleContentNotFoundException : BadRequestException, ILocalizableException
{
    public SaleContentNotFoundException(int id) : base(null, new { Id = id })
    {
    }

    public string MessageKey => "sale.content.not.found";
    public object[]? Arguments => null;
}