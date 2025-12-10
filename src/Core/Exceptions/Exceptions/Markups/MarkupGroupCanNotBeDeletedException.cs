using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Markups;

public class MarkupGroupCanNotBeDeletedException : BadRequestException
{
    [ExampleExceptionValues]
    public MarkupGroupCanNotBeDeletedException() : base("Нельзя удалить используемую политику наценок")
    {
    }
}