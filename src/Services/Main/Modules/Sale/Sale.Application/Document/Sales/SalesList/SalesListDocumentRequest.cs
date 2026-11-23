using System.Text.Json.Serialization;
using Application.Common.Models;
using Application.Common.Querying;
using Main.Application.Document.Document;
using Main.Entities.Sale;
using Main.Entities.Sale.Enums;
using Main.Modules.Sale.Application.Models;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Sale.Application.Document.Sales.SalesList;

public sealed record SalesListDocumentRequest : DocumentRequestBase
{
	[JsonPropertyName("rangeStartDate")]
	[SchemaInputControl(InputControlType.DatePicker)]
	[SchemaFieldLabel(SaleDocumentListRequestRangeStartDateNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListRequestRangeStartDateDescriptionMessage.Key)]
	public DateTime? RangeStartDate { get; init; }

	[JsonPropertyName("rangeEndDate")]
	[SchemaInputControl(InputControlType.DatePicker)]
	[SchemaFieldLabel(SaleDocumentListRequestRangeEndDateNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListRequestRangeEndDateDescriptionMessage.Key)]
	public DateTime? RangeEndDate { get; init; }

	[JsonPropertyName("organizationIds")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.Organization, "id")]
	[SchemaFieldLabel(SaleDocumentListRequestOrganizationIdsNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListRequestOrganizationIdsDescriptionMessage.Key)]
	public IReadOnlyCollection<Guid>? OrganizationIds { get; init; }

	[JsonPropertyName("currencyIds")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.Currency, "id")]
	[SchemaFieldLabel(SaleDocumentListRequestCurrencyIdsNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListRequestCurrencyIdsDescriptionMessage.Key)]
	public IReadOnlyCollection<int>? CurrencyIds { get; init; }

	[JsonPropertyName("productIds")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.Product, "id")]
	[SchemaFieldLabel(SaleDocumentListRequestProductIdsNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListRequestProductIdsDescriptionMessage.Key)]
	public IReadOnlyCollection<int>? ProductIds { get; init; }

	[JsonPropertyName("states")]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(SchemaEntity.SaleState)]
	[SchemaFieldLabel(SaleDocumentListRequestStatesNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListRequestStatesDescriptionMessage.Key)]
	public IReadOnlyCollection<SaleState>? States { get; init; }

	[JsonPropertyName("searchTerm")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SaleDocumentListRequestSearchTermNameMessage.Key)]
	[SchemaFieldDescription(SaleDocumentListRequestSearchTermDescriptionMessage.Key)]
	public string? SearchTerm { get; init; }

	public SalesFilter ToFilter()
		=> new()
		{
			DateRange = new RangeModel<DateTime>(RangeStartDate, RangeEndDate),
			OrganizationIds = OrganizationIds ?? [],
			CurrencyIds = CurrencyIds ?? [],
			ProductIds = ProductIds ?? [],
			States = States ?? [],
			SortBy = [new SortExpression("dateTime")],
			SearchTerm = SearchTerm
		};
}
