using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Main.Application.Lrts.Base;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.Lrts.ProductCrossesByIdsDelete;

public record ProductCrossesByIdsDeleteState : ProductCrossesByIdsDeleteInputState,
	ICsvImportState<ProductCrossesByIdsDeleteState>
{
	[JsonPropertyName("currentLine")]
	public int CurrentLine { get; init; }

	[JsonPropertyName("errors")]
	public List<CsvImportError> Errors { get; init; } = [];

	public ProductCrossesByIdsDeleteState WithCurrentLine(int currentLine) => this with
	{
		CurrentLine = currentLine
	};
}

[CsvSchema(typeof(ProductCrossesByIdsDeleteLrt.CsvRow))]
public record ProductCrossesByIdsDeleteInputState : IInputState, ICsvImportInputState
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
		if (!FileName.EndsWith(".csv", StringComparison.InvariantCulture))
			throw new InvalidOperationException(
				"Product crosses by IDs delete state error. File name should end with .csv");
	}
}
