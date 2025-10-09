namespace Core.Models.TimeWebCloud;

public class MailBoxModel
{
    /// <summary>
    ///     Автоответчик на входящие письма
    /// </summary>
    public AutoReplyModel AutoReplies { get; set; } = null!;

    /// <summary>
    ///     Спам-фильтр
    /// </summary>
    public SpamFilterModel SpamFilter { get; set; } = null!;

    /// <summary>
    ///     Пересылка входящих писем
    /// </summary>
    public ForwardingIncomingModel ForwardingIncoming { get; set; } = null!;

    /// <summary>
    ///     Пересылка исходящих писем
    /// </summary>
    public ForwardingOutgoingModel ForwardingOutgoing { get; set; } = null!;

    /// <summary>
    ///     Комментарий к почтовому ящику
    /// </summary>
    public string Comment { get; set; } = null!;

    /// <summary>
    ///     Домен почты
    /// </summary>
    public string Fqdn { get; set; } = null!;

    /// <summary>
    ///     Название почтового ящика
    /// </summary>
    public string MailBoxName { get; set; } = null!;

    /// <summary>
    ///     Пароль почтового ящика
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    ///     Использованное место на почтовом ящике (в Мб)
    /// </summary>
    public int UsageSpace { get; set; }

    /// <summary>
    ///     Доступен ли Webmail
    /// </summary>
    public bool IsWebMail { get; set; }

    /// <summary>
    ///     IDN домен почтового ящика
    /// </summary>
    public string IdnName { get; set; } = null!;

    /// <summary>
    ///     Есть ли доступ через dovecot
    /// </summary>
    public bool IsDovecot { get; set; }
}