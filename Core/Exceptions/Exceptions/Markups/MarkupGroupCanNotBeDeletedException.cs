using Exceptions.Base;

namespace Exceptions.Exceptions.Markups;

public class MarkupGroupCanNotBeDeletedException() : BadRequestException("Нельзя удалить используемую политику наценок")
{
}