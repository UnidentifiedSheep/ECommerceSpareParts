using System.Text.Json.Serialization;
using Favorit.Integrations.Core.Models;
using Integrations.Common;

namespace Favorit.Integrations.Core.Responses;

public record GetPricesResponse
{
    [JsonPropertyName("goods")]
    public IReadOnlyList<Good> Goods { get; init; } = [];
}