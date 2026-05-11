using System.Text.Json.Serialization;
using Abstractions.Models;

namespace Main.Application.Dtos.Storage;

public record PatchStorageContentDto
{
    [JsonPropertyName("count")]
    public PatchField<int> Count { get; set; } = PatchField<int>.NotSet();

    [JsonPropertyName("purchaseDatetime")]
    public PatchField<DateTime> PurchaseDatetime { get; set; } = PatchField<DateTime>.NotSet();

    [JsonPropertyName("buyPrice")]
    public PatchField<decimal> BuyPrice { get; set; } = PatchField<decimal>.NotSet();

    [JsonPropertyName("currencyId")]
    public PatchField<int> CurrencyId { get; set; } = PatchField<int>.NotSet();
}