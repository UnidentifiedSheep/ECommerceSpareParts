using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Pricing.Abstractions.Exceptions.Markup;

public class MarkupGroupNotFoundException : NotFoundException, ILocalizableException
{
    public MarkupGroupNotFoundException(int id) : base(null, new { Id = id })
    {
    }

    public string MessageKey => "markup.group.not.found";
    public object[]? Arguments => null;
}