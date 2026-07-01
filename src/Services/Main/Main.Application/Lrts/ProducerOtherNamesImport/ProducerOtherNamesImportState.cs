using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Attributes.JsonAttributes;
using Enums;

namespace Main.Application.Lrts.ProducerOtherNamesImport;

public record ProducerOtherNamesImportState
{
    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    [JsonPropertyName("currentLine")]
    public int CurrentLine { get; init; }

    [JsonPropertyName("errors")]
    public List<ProducerOtherNamesImportError> Errors { get; init; } = [];
}

[CsvSchema(typeof(ProducerOtherNamesImportLrt.ProducerOtherNameCsvDto))]
public record ProducerOtherNamesImportInputState : IInputState
{
    [Accepts(".csv")]
    [InputControl(InputControlType.UploadFile)]
    [RequiredJsonField]
    [LocalizedJsonFieldDescription("file_name_description")]
    [LocalizedJsonFieldName("file_name")]
    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    public void ValidateState()
    {
        if (!FileName.EndsWith(".csv"))
            throw new InvalidOperationException(
                "Producer other name import state error. " +
                "File name should end with .csv");
    }
}

public record ProducerOtherNamesImportError
{
    [JsonPropertyName("rowIdx")]
    public int RowIdx { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}