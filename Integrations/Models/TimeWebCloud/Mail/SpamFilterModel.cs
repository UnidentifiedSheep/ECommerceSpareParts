using Core.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Integrations.Models.TimeWebCloud.Mail;

[JsonObject("spam_filter")]
public class SpamFilterModel
{
    /// <summary>
    /// Включен ли спам-фильтр
    /// </summary>
    [JsonProperty("is_enabled")]
    public bool IsEnabled { get; set; }
    /// <summary>
    ///Что делать с письмами, которые попадают в спам.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("action")]
    public SpamFilterEnum Action { get; set; }
    /// <summary>
    /// Адрес для пересылки при выбранном действии forward из параметра action
    /// </summary>
    [JsonProperty("forward_to")]
    public string ForwardTo { get; set; } = null!;
    /// <summary>
    /// Белый список адресов от которых письма не будут попадать в спам
    /// </summary>
    [JsonProperty("white_list")]
    public IEnumerable<string> WhiteList { get; set; } = [];
}