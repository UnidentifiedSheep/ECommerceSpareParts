using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Attributes.JsonAttributes;
using Enums;

namespace Main.Application.Lrts.ProducerImport;

public record ProducerImportState
{
    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    [JsonPropertyName("currentLine")]
    public int CurrentLine { get; init; }

    [JsonPropertyName("errors")]
    public List<ProducerImportError> Errors { get; init; } = [];
}

public record ProducerImportInputState : IInputState
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
            throw new InvalidOperationException("Producer import state error. " +
                                                "File name should end with .csv");
    }
}

public record ProducerImportError
{
    [JsonPropertyName("rowIdx")]
    public int RowIdx { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}