using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Purchase;

public class PurchaseNotFoundException : NotFoundException, ILocalizableException
{
    public PurchaseNotFoundException(string id) : base(null, new { Id = id })
    {
    }

    public string MessageKey => "purchase.not.found";
    public object[]? Arguments => null;
}