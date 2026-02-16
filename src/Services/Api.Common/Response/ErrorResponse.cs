using System.Text.Json.Serialization;
using Abstractions.Models.Validation;
using Api.Common.Models;

namespace Api.Common.Response;

public class ErrorResponse
{
    [JsonPropertyName("statusCode")]
    public int Status { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
    [JsonPropertyName("instance")]
    public string? Instance { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; } = string.Empty;
    [JsonPropertyName("errors")]
    public List<ErrorResponse>? Errors { get; set; }
    [JsonPropertyName("validationErrors")]
    public List<ValidationErrorModel>? ValidationErrors { get; set; }
}