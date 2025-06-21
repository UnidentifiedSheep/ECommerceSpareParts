using System.ComponentModel.DataAnnotations;
using Core.Services.TimeWebCloud.Models.Mail;
using Newtonsoft.Json;

namespace Core.Services.TimeWebCloud.Responses;

public class GetMailsResponse
{
    /// <summary>
    /// Вспомогательная информация о возвращаемой сущности.
    /// </summary>
    public MetaModel Meta { get; set; } = null!;
    [JsonProperty("mailboxes")]
    public IEnumerable<MailBoxModel> MailBoxes { get; set; } = [];
    /// <summary>
    /// ID запроса, который можно указывать при обращении в службу технической поддержки, чтобы помочь определить проблему.
    /// </summary>
    [JsonProperty("response_id")]
    public string ResponseId { get; set; } = null!;
}

public class GetMailsOfDomainResponse
{
    /// <summary>
    /// Вспомогательная информация о возвращаемой сущности.
    /// </summary>
    public MetaModel Meta { get; set; } = null!;
    [JsonProperty("mailboxes")]
    public IEnumerable<MailBoxModel> MailBoxes { get; set; } = [];
    /// <summary>
    /// ID запроса, который можно указывать при обращении в службу технической поддержки, чтобы помочь определить проблему.
    /// </summary>
    [JsonProperty("response_id")]
    public string ResponseId { get; set; } = null!;
}