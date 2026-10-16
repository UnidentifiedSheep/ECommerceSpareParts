using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;
using Main.Application.Lrts.Base;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.Lrts.ProductCrossesByIdsImport;

public record ProductCrossesByIdsImportState : ProductCrossesByIdsImportInputState,
	ICsvImportState<ProductCrossesByIdsImportState>
{
	[JsonPropertyName("errors")]
	public List<CsvImportError> Errors { get; init; } = [];

	[JsonPropertyName("checkpoint")]
	public CsvImportCheckpoint? Checkpoint { get; init; }

	public ProductCrossesByIdsImportState WithCheckpoint(CsvImportCheckpoint checkpoint) => this with
	{
		Checkpoint = checkpoint
	};
}

[CsvSchema(typeof(ProductCrossesByIdsImportLrt.ProductCrossByIdsCsvDto))]
public record ProductCrossesByIdsImportInputState : IInputState, ICsvImportInputState
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
				"Product crosses by IDs import state error. File name should end with .csv");
	}
}
