namespace Core.Models.TimeWebCloud;

public class AutoReplyModel
{
    /// <summary>
    /// Включен ли автоответчик на входящие письма
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// Сообщение автоответчика на входящие письма
    /// </summary>
    public string Message { get; set; } = null!;
    /// <summary>
    /// Тема сообщения автоответчика на входящие письма
    /// </summary>
    public string Subject { get; set; } = null!;
}