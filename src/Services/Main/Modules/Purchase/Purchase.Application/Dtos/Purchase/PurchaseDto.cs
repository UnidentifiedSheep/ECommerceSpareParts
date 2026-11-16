using System.Text.Json.Serialization;

namespace Main.Modules.Purchase.Application.Dtos.Purchase;

public record PurchaseDto
{
	[JsonPropertyName("id")]
	public required Guid Id { get; init; }

	[JsonPropertyName("supplierId")]
	public required Guid SupplierId { get; init; }

	[JsonPropertyName("supplierOrganizationId")]
	public required Guid SupplierOrganizationId { get; init; }

	[JsonPropertyName("currencyId")]
	public required int CurrencyId { get; init; }

	[JsonPropertyName("comment")]
	public required string? Comment { get; init; }

	[JsonPropertyName("storageCode")]
	public required string StorageCode { get; init; }

	[JsonPropertyName("purchaseDatetime")]
	public required DateTime PurchaseDatetime { get; init; }

	[JsonPropertyName("transactionId")]
	public required Guid TransactionId { get; init; }

	[JsonPropertyName("totalSum")]
	public required decimal TotalSum { get; init; }
}
