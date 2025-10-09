using Newtonsoft.Json;

namespace Integrations.Models.TimeWebCloud.Mail;

[JsonObject("forwarding_outgoing")]
public class ForwardingOutgoingModel
{
    /// <summary>
    ///     Включена ли пересылка исходящих писем
    /// </summary>
    [JsonProperty("is_enabled")]
    public bool IsEnabled { get; set; }

    /// <summary>
    ///     Адрес для пересылки исходящих писем
    /// </summary>
    [JsonProperty("outgoing_to")]
    public string OutgoingTo { get; set; } = null!;
}