using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Attributes.JsonAttributes;
using Enums;
using Main.Application.Lrts.Base;

namespace Main.Application.Lrts.ProductImport;

public record ProductImportState : ProductImportInputState, ICsvImportState<ProductImportState>
{
    [JsonPropertyName("currentLine")]
    public int CurrentLine { get; init; }

    [JsonPropertyName("skippedLines")]
    public List<int> SkippedLines { get; init; } = [];

    [JsonPropertyName("errors")]
    public List<CsvImportError> Errors { get; init; } = [];

    public ProductImportState WithCurrentLine(int currentLine)
        => this with { CurrentLine = currentLine };
}

[CsvSchema(typeof(ProductImportLrt.NewProductCsvDto))]
public record ProductImportInputState : IInputState, ICsvImportInputState
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
                "Product import state error. " +
                "File name should end with .csv");
    }
}
