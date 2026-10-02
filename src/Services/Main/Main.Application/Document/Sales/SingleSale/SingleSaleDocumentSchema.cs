using System.Text.Json.Serialization;
using Main.Entities;
using Main.Enums;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Application.Document.Sales.SingleSale;

public sealed record SingleSaleDocumentSchema
{
	[JsonPropertyName(nameof(Id))]
	[SchemaFieldLabel(SaleDocumentSingleIdNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleIdDescriptionMessage.Key)]
	public required Guid Id { get; init; }

	[JsonPropertyName(nameof(SaleDatetime))]
	[SchemaFieldLabel(SaleDocumentSingleSaleDatetimeNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleSaleDatetimeDescriptionMessage.Key)]
	public required DateTime SaleDatetime { get; init; }

	[JsonPropertyName(nameof(TransactionId))]
	[SchemaFieldLabel(SaleDocumentSingleTransactionIdNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleTransactionIdDescriptionMessage.Key)]
	public required Guid TransactionId { get; init; }

	[JsonPropertyName(nameof(BuyerName))]
	[SchemaFieldLabel(SaleDocumentSingleBuyerNameNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleBuyerNameDescriptionMessage.Key)]
	public required string BuyerName { get; init; }

	[JsonPropertyName(nameof(OrganizationName))]
	[SchemaFieldLabel(SaleDocumentSingleOrganizationNameNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleOrganizationNameDescriptionMessage.Key)]
	public required string OrganizationName { get; init; }

	[JsonPropertyName(nameof(StorageCode))]
	[SchemaFieldLabel(SaleDocumentSingleStorageCodeNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleStorageCodeDescriptionMessage.Key)]
	public required string StorageCode { get; init; }

	[JsonPropertyName(nameof(CurrencyCode))]
	[SchemaFieldLabel(SaleDocumentSingleCurrencyCodeNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleCurrencyCodeDescriptionMessage.Key)]
	public required string CurrencyCode { get; init; }

	[JsonPropertyName(nameof(CurrencySign))]
	[SchemaFieldLabel(SaleDocumentSingleCurrencySignNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleCurrencySignDescriptionMessage.Key)]
	public required string CurrencySign { get; init; }

	[JsonPropertyName(nameof(State))]
	[SchemaFieldLabel(SaleDocumentSingleStateNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleStateDescriptionMessage.Key)]
	public required SaleState State { get; init; }

	[JsonPropertyName(nameof(TotalSum))]
	[SchemaFieldLabel(SaleDocumentSingleTotalSumNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleTotalSumDescriptionMessage.Key)]
	public required decimal TotalSum { get; init; }

	[JsonPropertyName(nameof(Comment))]
	[SchemaFieldLabel(SaleDocumentSingleCommentNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleCommentDescriptionMessage.Key)]
	public string? Comment { get; init; }

	[JsonPropertyName(nameof(Items))]
	[SchemaFieldLabel(SaleDocumentSingleItemsNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleItemsDescriptionMessage.Key)]
	public required IReadOnlyList<SingleSaleDocumentItemSchema> Items { get; init; }
}

public sealed record SingleSaleDocumentItemSchema
{
	[JsonPropertyName(nameof(ProductId))]
	[SchemaFieldLabel(SaleDocumentSingleItemProductIdNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleItemProductIdDescriptionMessage.Key)]
	public required int ProductId { get; init; }

	[JsonPropertyName(nameof(Sku))]
	[SchemaFieldLabel(SaleDocumentSingleItemSkuNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleItemSkuDescriptionMessage.Key)]
	public required string Sku { get; init; }

	[JsonPropertyName(nameof(ProductName))]
	[SchemaFieldLabel(SaleDocumentSingleItemProductNameNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleItemProductNameDescriptionMessage.Key)]
	public required string ProductName { get; init; }

	[JsonPropertyName(nameof(Count))]
	[SchemaFieldLabel(SaleDocumentSingleItemCountNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleItemCountDescriptionMessage.Key)]
	public required int Count { get; init; }

	[JsonPropertyName(nameof(Price))]
	[SchemaFieldLabel(SaleDocumentSingleItemPriceNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleItemPriceDescriptionMessage.Key)]
	public required decimal Price { get; init; }

	[JsonPropertyName(nameof(Discount))]
	[SchemaFieldLabel(SaleDocumentSingleItemDiscountNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleItemDiscountDescriptionMessage.Key)]
	public required decimal Discount { get; init; }

	[JsonPropertyName(nameof(TotalSum))]
	[SchemaFieldLabel(SaleDocumentSingleItemTotalSumNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleItemTotalSumDescriptionMessage.Key)]
	public required decimal TotalSum { get; init; }

	[JsonPropertyName(nameof(Comment))]
	[SchemaFieldLabel(SaleDocumentSingleItemCommentNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentSingleItemCommentDescriptionMessage.Key)]
	public string? Comment { get; init; }
}
