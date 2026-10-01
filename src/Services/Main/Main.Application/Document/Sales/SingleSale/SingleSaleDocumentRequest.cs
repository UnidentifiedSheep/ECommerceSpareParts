using System.Text.Json.Serialization;

namespace Main.Application.Document.Sales.SingleSale;

public sealed record SingleSaleDocumentRequest : DocumentRequestBase
{
	[JsonPropertyName("saleId")]
	public required Guid SaleId { get; init; }
}
