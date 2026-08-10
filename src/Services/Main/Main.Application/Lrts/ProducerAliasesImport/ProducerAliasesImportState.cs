using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Attributes.JsonAttributes;
using Enums;
using Main.Application.Lrts.Base;

namespace Main.Application.Lrts.ProducerAliasesImport;

public record ProducerAliasesImportState : ProducerAliasesImportInputState,
    ICsvImportState<ProducerAliasesImportState>
{
    [JsonPropertyName("currentLine")]
    public int CurrentLine { get; init; }

    [JsonPropertyName("errors")]
    public List<CsvImportError> Errors { get; init; } = [];

    public ProducerAliasesImportState WithCurrentLine(int currentLine)
        => this with { CurrentLine = currentLine };
}

[CsvSchema(typeof(ProducerAliasImportLrt.ProducerAliasCsvDto))]
public record ProducerAliasesImportInputState : IInputState, ICsvImportInputState
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
