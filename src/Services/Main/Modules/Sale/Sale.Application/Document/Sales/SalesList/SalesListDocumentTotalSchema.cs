using System.Text.Json.Serialization;
using Main.Entities.Sale;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Sale.Application.Document.Sales.SalesList;

public sealed record SalesListDocumentTotalSchema
{
	[JsonPropertyName(nameof(CurrencyCode))]
	[SchemaFieldLabel(SaleDocumentListTotalsCurrencyCodeNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListTotalsCurrencyCodeDescriptionMessage.Key)]
	public required string CurrencyCode { get; init; }

	[JsonPropertyName(nameof(CurrencySign))]
	[SchemaFieldLabel(SaleDocumentListTotalsCurrencySignNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListTotalsCurrencySignDescriptionMessage.Key)]
	public required string CurrencySign { get; init; }

	[JsonPropertyName(nameof(SalesCount))]
	[SchemaFieldLabel(SaleDocumentListTotalsSalesCountNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListTotalsSalesCountDescriptionMessage.Key)]
	public required int SalesCount { get; init; }

	[JsonPropertyName(nameof(ItemsCount))]
	[SchemaFieldLabel(SaleDocumentListTotalsItemsCountNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListTotalsItemsCountDescriptionMessage.Key)]
	public required int ItemsCount { get; init; }

	[JsonPropertyName(nameof(TotalSum))]
	[SchemaFieldLabel(SaleDocumentListTotalsSumNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListTotalsSumDescriptionMessage.Key)]
	public required decimal TotalSum { get; init; }
}
