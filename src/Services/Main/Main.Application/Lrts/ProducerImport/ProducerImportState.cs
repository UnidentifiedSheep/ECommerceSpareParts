using System.Text.Json.Serialization;
using Attributes.JsonAttributes;

namespace Main.Application.Lrts.ProducerImport;

public record ProducerImportState
{
    [JsonPropertyName("fileName")]
    [LocalizableJsonPropertyName("file_name")]
    public required string FileName { get; init; }

    [JsonPropertyName("currentLine")]
    [LocalizableJsonPropertyName("current_line")]
    public int CurrentLine { get; init; }

    [JsonPropertyName("errors")]
    [LocalizableJsonPropertyName("errors")]
    public List<ProducerImportError> Errors { get; init; } = [];
}

public record ProducerImportError
{
    [JsonPropertyName("rowIdx")]
    [LocalizableJsonPropertyName("row_idx")]
    public int RowIdx { get; init; }

    [JsonPropertyName("message")]
    [LocalizableJsonPropertyName("message")]
    public required string Message { get; init; }
}