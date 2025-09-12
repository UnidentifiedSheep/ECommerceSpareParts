namespace Core.Models.TimeWebCloud;

public class ForwardingIncomingModel
{
    /// <summary>
    ///     Включена ли пересылка входящик писем
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    ///     Удалять ли входящие письма
    /// </summary>
    public bool IsDeleteMessages { get; set; }

    /// <summary>
    ///     Список адресов для пересылки
    /// </summary>
    public IEnumerable<string> IncomingList { get; set; } = [];
}