using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Purchase;

public class PurchaseContentNotFoundException : NotFoundException, ILocalizableException
{
    public PurchaseContentNotFoundException(int id) : base(null, new { Id = id })
    {
    }

    public string MessageKey => "purchase.content.not.found";
    public object[]? Arguments => null;
}