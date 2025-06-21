using Newtonsoft.Json;

namespace Core.Services.Armtek.Models;

public class UserInfoModel
{
    [JsonProperty("kunag")] public string Client { get; set; } = null!;
    [JsonProperty("vkorg")] public string Vkorg { get; set; } = null!;
    [JsonProperty("sname")] public string ShortName { get; set; } = null!;
    [JsonProperty("fname")] public string FullName { get; set; } = null!;
    [JsonProperty("adress")] public string Address { get; set; } = null!;
    [JsonProperty("phone")] public string Phone { get; set; } = null!;
    public List<RgTab> RgTabs { get; set; } = null!;
    public List<ZaTab> ZaTabs { get; set; } = null!;
    public List<ExwTab> ExwTabs { get; set; } = null!;
    public List<DogovorTab> DogovorTabs { get; set; } = null!;
    public List<ContactTab> ContactTabs { get; set; } = null!;
}