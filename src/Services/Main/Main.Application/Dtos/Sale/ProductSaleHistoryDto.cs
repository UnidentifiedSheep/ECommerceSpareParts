using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Sale;

public record ProductSaleHistoryDto
{
    [JsonPropertyName("saleContentId")]
    public required int SaleContentId { get; init; }

    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    
    [JsonPropertyName("organizationId")]
    public required Guid OrganizationId { get; init; }

    [JsonPropertyName("storageCode")]
    public required string StorageCode { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }

    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }

    [JsonPropertyName("discount")]
    public required decimal Discount { get; init; }

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }

    [JsonPropertyName("averageBuyPrice")]
    public required decimal AverageBuyPrice { get; init; }

    [JsonPropertyName("saleDate")]
    public required DateTime SaleDate { get; init; }

    [JsonPropertyName("whoCreated")]
    public Guid? WhoCreated { get; init; }
}
