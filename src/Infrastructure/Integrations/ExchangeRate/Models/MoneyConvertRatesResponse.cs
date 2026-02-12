using System.Text.Json.Serialization;

namespace ExchangeRate.Models;

public class MoneyConvertRatesResponse
{
    [JsonPropertyName("base")]
    public string Base { get; set; } = null!;
    
    [JsonPropertyName("disclaimer")] 
    public string Disclaimer { get; set; } = null!;
    
    [JsonPropertyName("license")] 
    public string License { get; set; } = null!;

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
    
    [JsonPropertyName("source")] 
    public string Source { get; set; } = null!;

    [JsonPropertyName("rates")] 
    public Dictionary<string, decimal> Rates { get; set; } = [];
}