using System.Text.Json.Serialization;
using Main.Entities.Sale;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Sale.Application.Document.Sales.SalesList;

public sealed record SalesListDocumentSaleSchema
{
	[JsonPropertyName(nameof(SaleDatetime))]
	[SchemaFieldLabel(SaleDocumentListSaleDatetimeNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListSaleDatetimeDescriptionMessage.Key)]
	public required DateTime SaleDatetime { get; init; }

	[JsonPropertyName(nameof(CurrencyCode))]
	[SchemaFieldLabel(SaleDocumentListCurrencyCodeNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListCurrencyCodeDescriptionMessage.Key)]
	public required string CurrencyCode { get; init; }

	[JsonPropertyName(nameof(CurrencySign))]
	[SchemaFieldLabel(SaleDocumentListCurrencySignNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListCurrencySignDescriptionMessage.Key)]
	public required string CurrencySign { get; init; }

	[JsonPropertyName(nameof(StorageCode))]
	[SchemaFieldLabel(SaleDocumentListStorageCodeNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListStorageCodeDescriptionMessage.Key)]
	public required string StorageCode { get; init; }

	[JsonPropertyName(nameof(BuyerName))]
	[SchemaFieldLabel(SaleDocumentListBuyerNameNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListBuyerNameDescriptionMessage.Key)]
	public required string BuyerName { get; init; }

	[JsonPropertyName(nameof(TotalSum))]
	[SchemaFieldLabel(SaleDocumentListTotalSumNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListTotalSumDescriptionMessage.Key)]
	public required decimal TotalSum { get; init; }

	[JsonPropertyName(nameof(Comment))]
	[SchemaFieldLabel(SaleDocumentListCommentNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListCommentDescriptionMessage.Key)]
	public string? Comment { get; init; }

	[JsonPropertyName(nameof(Items))]
	[SchemaFieldLabel(SaleDocumentListItemsNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListItemsDescriptionMessage.Key)]
	public required IReadOnlyList<SalesListDocumentItemSchema> Items { get; init; }
}
