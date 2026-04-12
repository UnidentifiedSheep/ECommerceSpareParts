using System.Text.Json.Serialization;
using Abstractions.Models;

namespace Main.Abstractions.Dtos.Amw.Articles;

public class PatchProductDto
{
    [JsonPropertyName("sku")]
    public PatchField<string> Sku { get; set; } = PatchField<string>.NotSet();
    
    [JsonPropertyName("name")]
    public PatchField<string> Name { get; set; } = PatchField<string>.NotSet();
    
    [JsonPropertyName("productId")]
    public PatchField<int> ProducerId { get; set; } = PatchField<int>.NotSet();
    
    [JsonPropertyName("description")]
    public PatchField<string?> Description { get; set; } = PatchField<string?>.NotSet();
    
    [JsonPropertyName("packingUnit")]
    public PatchField<int?> PackingUnit { get; set; } = PatchField<int?>.NotSet();
    
    [JsonPropertyName("indicator")]
    public PatchField<string?> Indicator { get; set; } = PatchField<string?>.NotSet();
    
    [JsonPropertyName("categoryId")]
    public PatchField<int?> CategoryId { get; set; } = PatchField<int?>.NotSet();
    
    [JsonPropertyName("pairId")]
    public PatchField<int?> PairId { get; set; } = PatchField<int?>.NotSet();
}