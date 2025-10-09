using Core.Enums;

namespace Core.Models.TimeWebCloud;

public class SpamFilterModel
{
    /// <summary>
    ///     Включен ли спам-фильтр
    /// </summary>

    public bool IsEnabled { get; set; }

    /// <summary>
    ///     Что делать с письмами, которые попадают в спам.
    /// </summary>
    public SpamFilterEnum Action { get; set; }

    /// <summary>
    ///     Адрес для пересылки при выбранном действии forward из параметра action
    /// </summary>
    public string ForwardTo { get; set; } = null!;

    /// <summary>
    ///     Белый список адресов от которых письма не будут попадать в спам
    /// </summary>
    public IEnumerable<string> WhiteList { get; set; } = [];
}