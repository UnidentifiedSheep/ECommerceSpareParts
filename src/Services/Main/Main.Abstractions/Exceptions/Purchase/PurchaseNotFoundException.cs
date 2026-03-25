using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Purchase;

public class PurchaseNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "purchase.not.found";
    public object[]? Arguments => null;
    public PurchaseNotFoundException(string id) : base(null, new { Id = id })
    {
    }
}