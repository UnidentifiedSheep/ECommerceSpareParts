using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Markups;

public class SelectedMarkupNotFoundException : NotFoundException
{
    [ExampleExceptionValues]
    public SelectedMarkupNotFoundException() : base("Не удалось найти выбранную стратегию наценки")
    {
    }
}