namespace Core.Models.TimeWebCloud;

public class ForwardingOutgoingModel
{
    /// <summary>
    ///     Включена ли пересылка исходящих писем
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    ///     Адрес для пересылки исходящих писем
    /// </summary>
    public string OutgoingTo { get; set; } = null!;
}