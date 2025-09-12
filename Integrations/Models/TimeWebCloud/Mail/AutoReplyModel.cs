using Newtonsoft.Json;

namespace Integrations.Models.TimeWebCloud.Mail;

[JsonObject("auto_reply")]
public class AutoReplyModel
{
    /// <summary>
    /// Включен ли автоответчик на входящие письма
    /// </summary>
    [JsonProperty("is_enabled")]
    public bool IsEnabled { get; set; }
    /// <summary>
    /// Сообщение автоответчика на входящие письма
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; } = null!;
    /// <summary>
    /// Тема сообщения автоответчика на входящие письма
    /// </summary>
    [JsonProperty("subject")]
    public string Subject { get; set; } = null!;
}