using Newtonsoft.Json;

namespace Integrations.Models.Armtek;

public class DogovorTab
{
    [JsonProperty("vbeln")] public string Dogovor { get; set; } = null!;
    [JsonProperty("bstkd")] public string DogovorName { get; set; } = null!;
    [JsonProperty("bstkdt")] public string SpecialCondition { get; set; } = null!;
    [JsonProperty("bstdk")] public string DogovorDateTime { get; set; } = null!;
    [JsonProperty("datbi")] public string DogovorValidityTime { get; set; } = null!;
    [JsonProperty("default")] public string Default { get; set; } = null!;
    [JsonProperty("auart")] public string DogovorType { get; set; } = null!;
    [JsonProperty("klimk")] public string TotalCreditLimit { get; set; } = null!;
    [JsonProperty("klimku")] public string CreditLeft { get; set; } = null!;
    [JsonProperty("oeikw")] public string ReservedItems { get; set; } = null!;
    [JsonProperty("waers")] public string Currency { get; set; } = null!;
    [JsonProperty("zterm")] public string MaximumPaymentDelay { get; set; } = null!;
    [JsonProperty("scale_tab")] public List<ScaleTab> ScaleTabs { get; set; } = null!;
}