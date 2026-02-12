using Exceptions.Base;

namespace Exceptions.Exceptions.Markups;

public class MarkupGroupCanNotBeDeletedException : BadRequestException
{
    public MarkupGroupCanNotBeDeletedException() : base("Нельзя удалить используемую политику наценок")
    {
    }
}