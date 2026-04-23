using System.Text.Json.Serialization;
using Main.Enums;

namespace Main.Application.Dtos.Product;

public record NewProductLinkageDto
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }
    
    [JsonPropertyName("crossProductId")]
    public int CrossProductId { get; set; }
    
    [JsonPropertyName("linkageType")]
    public ProductLinkageType LinkageType { get; set; }
}