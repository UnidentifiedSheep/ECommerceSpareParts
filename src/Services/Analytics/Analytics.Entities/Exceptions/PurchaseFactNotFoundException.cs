using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Analytics.Entities.Exceptions;

public class PurchaseFactNotFoundException(string id)
    : NotFoundException(null, new { Id = id }), ILocalizableException
{
    public string MessageKey => "purchase.fact.not.found";
    public object[]? Arguments => null;
}