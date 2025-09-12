using Core.Models.TimeWebCloud;

namespace Core.Dtos.TimeWebCloud.Responses;

public class GetMailsOfDomainResponse
{
    /// <summary>
    /// Вспомогательная информация о возвращаемой сущности.
    /// </summary>
    public MetaModel Meta { get; set; } = null!;
    public IEnumerable<MailBoxModel> MailBoxes { get; set; } = [];
    /// <summary>
    /// ID запроса, который можно указывать при обращении в службу технической поддержки, чтобы помочь определить проблему.
    /// </summary>
    public string ResponseId { get; set; } = null!;
}