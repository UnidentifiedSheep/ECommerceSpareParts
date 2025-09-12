using Newtonsoft.Json;

namespace Integrations.Models.TimeWebCloud.Mail;

[JsonObject("forwarding_incoming")]
public class ForwardingIncomingModel
{
    /// <summary>
    ///     Включена ли пересылка входящик писем
    /// </summary>
    [JsonProperty("is_enabled")]
    public bool IsEnabled { get; set; }

    /// <summary>
    ///     Удалять ли входящие письма
    /// </summary>
    [JsonProperty("is_delete_messages")]
    public bool IsDeleteMessages { get; set; }

    /// <summary>
    ///     Список адресов для пересылки
    /// </summary>
    [JsonProperty("incoming_list")]
    public IEnumerable<string> IncomingList { get; set; } = [];
}