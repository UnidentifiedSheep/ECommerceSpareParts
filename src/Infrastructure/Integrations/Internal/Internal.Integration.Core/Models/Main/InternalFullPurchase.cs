using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main;

public record InternalFullPurchase
{
    [JsonPropertyName("purchase")]
    public required InternalPurchase Purchase { get; init; }

    [JsonPropertyName("contents")]
    public required IReadOnlyList<InternalPurchaseContent> Contents { get; init; }
}

public record InternalPurchase
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("supplier")]
    public required InternalUser Supplier { get; init; }

    [JsonPropertyName("currency")]
    public required InternalCurrency Currency { get; init; }

    [JsonPropertyName("comment")]
    public required string? Comment { get; init; }

    [JsonPropertyName("storage")]
    public required string Storage { get; init; }

    [JsonPropertyName("purchaseDatetime")]
    public required DateTime PurchaseDatetime { get; init; }

    [JsonPropertyName("transactionId")]
    public required Guid TransactionId { get; init; }

    [JsonPropertyName("totalSum")]
    public required decimal TotalSum { get; init; }

    [JsonPropertyName("logistics")]
    public required InternalPurchaseLogistic? Logistics { get; init; }
}

public record InternalPurchaseContent
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }

    [JsonPropertyName("totalSum")]
    public required decimal TotalSum { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }

    [JsonPropertyName("product")]
    public required InternalProduct Product { get; init; }

    [JsonPropertyName("logistics")]
    public required InternalPurchaseContentLogistic? ContentLogistics { get; init; }
}

public record InternalPurchaseLogistic
{
    [JsonPropertyName("routeId")]
    public required Guid RouteId { get; init; }

    [JsonPropertyName("transactionId")]
    public required Guid? TransactionId { get; init; }

    [JsonPropertyName("pricingModel")]
    public required string PricingModel { get; init; }

    [JsonPropertyName("routeType")]
    public required string RouteType { get; init; }

    [JsonPropertyName("priceKg")]
    public required decimal PriceKg { get; init; }

    [JsonPropertyName("pricePerM3")]
    public required decimal PricePerM3 { get; init; }

    [JsonPropertyName("pricePerOrder")]
    public required decimal PricePerOrder { get; init; }

    [JsonPropertyName("minimumPrice")]
    public required decimal? MinimumPrice { get; init; }

    [JsonPropertyName("minimumPriceApplied")]
    public required bool MinimumPriceApplied { get; init; }

    [JsonPropertyName("currency")]
    public required InternalCurrency Currency { get; init; }
}

public record InternalPurchaseContentLogistic
{
    [JsonPropertyName("weightKg")]
    public required decimal WeightKg { get; init; }

    [JsonPropertyName("areaM3")]
    public required decimal AreaM3 { get; init; }

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }
}
