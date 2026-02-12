using Exceptions.Base;

namespace Exceptions.Exceptions.Markups;

public class SelectedMarkupNotFoundException : NotFoundException
{
    public SelectedMarkupNotFoundException() : base("Не удалось найти выбранную стратегию наценки")
    {
    }
}