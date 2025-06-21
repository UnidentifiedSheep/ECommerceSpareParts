using Newtonsoft.Json;

namespace Core.Services.TimeWebCloud.Models.Mail;

/// <summary>
/// Вспомогательная информация о возвращаемой сущности.
/// </summary>
[JsonObject("meta")]
public class MetaModel
{
    /// <summary>
    /// Общее количество элементов в коллекции.
    /// </summary>
    [JsonProperty("total")]
    public int Total { get; set; }
}