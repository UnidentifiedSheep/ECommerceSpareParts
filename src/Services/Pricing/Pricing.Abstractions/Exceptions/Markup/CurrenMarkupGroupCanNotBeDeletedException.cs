using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Pricing.Abstractions.Exceptions.Markup;

public class CurrenMarkupGroupCanNotBeDeletedException() 
    : BadRequestException(null), ILocalizableException
{
    public string MessageKey => "current.markup.group.can.not.be.deleted";
    public object[]? Arguments => null;
}