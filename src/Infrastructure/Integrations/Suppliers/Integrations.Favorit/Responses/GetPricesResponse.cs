using System.Text.Json.Serialization;
using Integrations.Favorit.Models;

namespace Integrations.Favorit.Responses;

public record GetPricesResponse
{
    [JsonPropertyName("goods")]
    public IReadOnlyList<Good> Goods { get; init; } = [];
}