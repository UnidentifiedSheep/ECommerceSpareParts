using System.Text.Json.Serialization;

namespace ExchangeRate.Models;

public class CbrRatesResponse
{
    [JsonPropertyName("disclaimer")] 
    public string Disclaimer { get; set; } = null!;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; } = null!;

    [JsonPropertyName("rates")] 
    public Dictionary<string, decimal> Rates { get; set; } = [];
}