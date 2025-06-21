using Newtonsoft.Json;

namespace Core.Services.TimeWebCloud.Models.Mail;

[JsonObject("mailbox")]
public class MailBoxModel
{
    /// <summary>
    /// Автоответчик на входящие письма
    /// </summary>
    public AutoReplyModel AutoReplies { get; set; } = null!;
    /// <summary>
    /// Спам-фильтр
    /// </summary>
    public SpamFilterModel SpamFilter  { get; set; } = null!;
    /// <summary>
    /// Пересылка входящих писем
    /// </summary>
    public ForwardingIncomingModel ForwardingIncoming { get; set; } = null!;
    /// <summary>
    /// Пересылка исходящих писем
    /// </summary>
    public ForwardingOutgoingModel ForwardingOutgoing { get; set; } = null!;
    /// <summary>
    /// Комментарий к почтовому ящику
    /// </summary>
    [JsonProperty("comment")]
    public string Comment { get; set; } = null!;
    /// <summary>
    /// Домен почты
    /// </summary>
    [JsonProperty("fqdn")]
    public string Fqdn { get; set; } = null!;
    /// <summary>
    /// Название почтового ящика
    /// </summary>
    [JsonProperty("mailbox")]
    public string MailBoxName { get; set; } = null!;
    /// <summary>
    /// Пароль почтового ящика
    /// </summary>
    [JsonProperty("password")]
    public string Password { get; set; } = null!;
    /// <summary>
    /// Использованное место на почтовом ящике (в Мб)
    /// </summary>
    [JsonProperty("usage_space")]
    public int UsageSpace  { get; set; }
    /// <summary>
    /// Доступен ли Webmail
    /// </summary>
    [JsonProperty("is_webmail")]
    public bool IsWebMail { get; set; }
    /// <summary>
    /// IDN домен почтового ящика
    /// </summary>
    [JsonProperty("idn_name")]
    public string IdnName { get; set; } = null!;
    /// <summary>
    /// Есть ли доступ через dovecot
    /// </summary>
    [JsonProperty("is_dovecot")]
    public bool IsDovecot { get; set; }
}