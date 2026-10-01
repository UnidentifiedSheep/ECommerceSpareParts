using System.Text.Json.Serialization;

namespace Main.Application.Document.Sales.SinglSale;

public sealed record SingleSaleDocumentRequest : DocumentRequestBase
{
	[JsonPropertyName("saleId")]
	public required Guid SaleId { get; init; }
}
