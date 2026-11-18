using System.Text.Json.Serialization;
using Main.Application.Document.Document;
using Main.Entities.Sale;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Sale.Application.Document.Sales.SingleSale;

public sealed record SingleSaleDocumentRequest : DocumentRequestBase
{
	[JsonPropertyName("saleId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.Sale, "id")]
	[SchemaFieldLabel(SaleDocumentSingleRequestSaleIdNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleRequestSaleIdDescriptionMessage.Key)]
	public required Guid SaleId { get; init; }
}
