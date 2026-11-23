using System.Text.Json.Serialization;
using Main.Entities.Sale;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Sale.Application.Document.Sales.SalesList;

public sealed record SalesListDocumentSchema
{
	[JsonPropertyName(nameof(Period))]
	[SchemaFieldLabel(SaleDocumentListPeriodNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListPeriodDescriptionMessage.Key)]
	public required string Period { get; init; }

	[JsonPropertyName(nameof(GeneratedAt))]
	[SchemaFieldLabel(SaleDocumentListGeneratedAtNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListGeneratedAtDescriptionMessage.Key)]
	public required DateTime GeneratedAt { get; init; }

	[JsonPropertyName(nameof(OrganizationsCount))]
	[SchemaFieldLabel(SaleDocumentListOrganizationsCountNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListOrganizationsCountDescriptionMessage.Key)]
	public required int OrganizationsCount { get; init; }

	[JsonPropertyName(nameof(SalesCount))]
	[SchemaFieldLabel(SaleDocumentListSalesCountNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListSalesCountDescriptionMessage.Key)]
	public required int SalesCount { get; init; }

	[JsonPropertyName(nameof(ItemsCount))]
	[SchemaFieldLabel(SaleDocumentListItemsCountNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListItemsCountDescriptionMessage.Key)]
	public required int ItemsCount { get; init; }

	[JsonPropertyName(nameof(Totals))]
	[SchemaFieldLabel(SaleDocumentListTotalsNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListTotalsDescriptionMessage.Key)]
	public required IReadOnlyList<SalesListDocumentTotalSchema> Totals { get; init; }

	[JsonPropertyName(nameof(Organizations))]
	[SchemaFieldLabel(SaleDocumentListOrganizationsNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListOrganizationsDescriptionMessage.Key)]
	public required IReadOnlyList<SalesListDocumentOrganizationSchema> Organizations { get; init; }
}
