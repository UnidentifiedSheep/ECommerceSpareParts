using System.Text.Json.Serialization;
using Main.Entities.Sale;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Sale.Application.Document.Sales.SalesList;

public sealed record SalesListDocumentOrganizationSchema
{
	[JsonPropertyName(nameof(OrganizationName))]
	[SchemaFieldLabel(SaleDocumentListOrganizationNameNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListOrganizationNameDescriptionMessage.Key)]
	public required string OrganizationName { get; init; }

	[JsonPropertyName(nameof(SalesCount))]
	[SchemaFieldLabel(SaleDocumentListOrganizationSalesCountNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListOrganizationSalesCountDescriptionMessage.Key)]
	public required int SalesCount { get; init; }

	[JsonPropertyName(nameof(TotalsText))]
	[SchemaFieldLabel(SaleDocumentListOrganizationTotalsTextNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListOrganizationTotalsTextDescriptionMessage.Key)]
	public required string TotalsText { get; init; }

	[JsonPropertyName(nameof(Sales))]
	[SchemaFieldLabel(SaleDocumentListOrganizationSalesNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListOrganizationSalesDescriptionMessage.Key)]
	public required IReadOnlyList<SalesListDocumentSaleSchema> Sales { get; init; }
}
