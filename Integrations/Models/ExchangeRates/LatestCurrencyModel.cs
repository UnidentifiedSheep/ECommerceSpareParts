using Newtonsoft.Json;

namespace Integrations.Models.ExchangeRates;

public class LatestCurrencyModel
{
    [JsonProperty("base")]
    public string BaseCurrency { get; set; } = null!;
    [JsonProperty("date")]
    public string Date { get; set; } = null!;
    [JsonProperty("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = null!;
    [JsonProperty("success")]
    public bool Success { get; set; } = false;
    [JsonProperty("timestamp")]
    public string Timestamp { get; set; } = null!;
    
}