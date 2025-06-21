using Core.Json;
using Newtonsoft.Json;

namespace Core.Services.TimeWebCloud.Models;

public class ExceptionModel
{
    [JsonProperty("status_code")]
    public int StatusCode { get; set; }
    [JsonProperty("message")]
    [JsonConverter(typeof(StringOrArrayConverter))]
    public List<string> Message { get; set; } = [];
    [JsonProperty("error_code")]
    public string ErrorCode { get; set; } = string.Empty;
    [JsonProperty("response_id")]
    public string ResponseId { get; set; } = string.Empty;
}