using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Attributes.JsonAttributes;
using Enums;
using Main.Application.Lrts.Base;

namespace Main.Application.Lrts.ProductCrossesImport;

public record ProductCrossesImportState : ProductCrossesImportInputState,
    ICsvImportState<ProductCrossesImportState>
{
    [JsonPropertyName("currentLine")]
    public int CurrentLine { get; init; }

    [JsonPropertyName("skippedLines")]
    public List<int> SkippedLines { get; init; } = [];

    [JsonPropertyName("errors")]
    public List<CsvImportError> Errors { get; init; } = [];

    public ProductCrossesImportState WithCurrentLine(int currentLine)
        => this with { CurrentLine = currentLine };
}

[CsvSchema(typeof(ProductCrossesImportLrt.ProductCrossCsvDto))]
public record ProductCrossesImportInputState : IInputState, ICsvImportInputState
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
                "Product crosses import state error. " +
                "File name should end with .csv");
    }
}
