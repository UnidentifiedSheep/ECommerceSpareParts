using Newtonsoft.Json;

namespace Core.Services.Armtek.Models;

public class ContactTab
{
    [JsonProperty("parnr")] public string PartnerNumber { get; set; } = null!;
    [JsonProperty("default")] public string Default { get; set; } = null!;
    [JsonProperty("fname")] public string FullName { get; set; } = null!;
    [JsonProperty("lname")] public string LastName { get; set; } = null!;
    [JsonProperty("mname")] public string MiddleName { get; set; } = null!;
    [JsonProperty("phone")] public string Phone { get; set; } = null!;
    [JsonProperty("email")] public string Email { get; set; } = null!;
}