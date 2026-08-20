using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;
using Main.Application.Lrts.Base;

namespace Main.Application.Lrts.ProducerSupplierMappingImport;

public record ProducerSupplierMappingImportState : ProducerSupplierMappingImportInputState,
    ICsvImportState<ProducerSupplierMappingImportState>
{
    [JsonPropertyName("currentLine")]
    public int CurrentLine { get; init; }

    [JsonPropertyName("errors")]
    public List<CsvImportError> Errors { get; init; } = [];

    public ProducerSupplierMappingImportState WithCurrentLine(int currentLine)
        => this with { CurrentLine = currentLine };
}

[CsvSchema(typeof(ProducerSupplierMappingImportLrt.ProducerSupplierMappingCsvDto))]
public record ProducerSupplierMappingImportInputState : IInputState, ICsvImportInputState
{
    [SchemaAccepts(".csv")]
    [SchemaInputControl(InputControlType.UploadFile)]
    [RequiredSchemaField]
    [SchemaFieldDescription("file_name_description")]
    [SchemaFieldLabel("file_name")]
    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    public void ValidateState()
    {
        if (!FileName.EndsWith(".csv"))
            throw new InvalidOperationException(
                "Supplier producer import state error. " +
                "File name should end with .csv");
    }
}
