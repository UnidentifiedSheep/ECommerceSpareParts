using System.Text.Json.Serialization;
using Main.Entities.Sale;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Sale.Application.Document.Sales.SalesList;

public sealed record SalesListDocumentItemSchema
{
	[JsonPropertyName(nameof(Sku))]
	[SchemaFieldLabel(SaleDocumentListItemSkuNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListItemSkuDescriptionMessage.Key)]
	public required string Sku { get; init; }

	[JsonPropertyName(nameof(ProductName))]
	[SchemaFieldLabel(SaleDocumentListItemProductNameNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListItemProductNameDescriptionMessage.Key)]
	public required string ProductName { get; init; }

	[JsonPropertyName(nameof(Count))]
	[SchemaFieldLabel(SaleDocumentListItemCountNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListItemCountDescriptionMessage.Key)]
	public required int Count { get; init; }

	[JsonPropertyName(nameof(OriginalPrice))]
	[SchemaFieldLabel(SaleDocumentListItemOriginalPriceNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListItemOriginalPriceDescriptionMessage.Key)]
	public required decimal OriginalPrice { get; init; }

	[JsonPropertyName(nameof(DiscountedPrice))]
	[SchemaFieldLabel(SaleDocumentListItemDiscountedPriceNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListItemDiscountedPriceDescriptionMessage.Key)]
	public required decimal DiscountedPrice { get; init; }

	[JsonPropertyName(nameof(TotalSum))]
	[SchemaFieldLabel(SaleDocumentListItemTotalSumNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListItemTotalSumDescriptionMessage.Key)]
	public required decimal TotalSum { get; init; }

	[JsonPropertyName(nameof(Comment))]
	[SchemaFieldLabel(SaleDocumentListItemCommentNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListItemCommentDescriptionMessage.Key)]
	public string? Comment { get; init; }
}
