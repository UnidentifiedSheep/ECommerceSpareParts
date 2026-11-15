using System.Text.Json.Serialization;
using Main.Entities;
using Main.Entities.Sale;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.Document.Sales.SingleSale;

public sealed record SingleSaleDocumentRequest : DocumentRequestBase
{
	[JsonPropertyName("saleId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(nameof(Sale), "id")]
	[SchemaFieldLabel(SaleDocumentSingleRequestSaleIdNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleRequestSaleIdDescriptionMessage.Key)]
	public required Guid SaleId { get; init; }
}
