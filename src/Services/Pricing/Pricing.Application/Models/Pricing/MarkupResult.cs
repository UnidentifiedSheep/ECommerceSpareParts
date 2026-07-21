using System.Text.Json.Serialization;

namespace Pricing.Application.Models.Pricing;

public sealed record MarkupResult
{
    [JsonPropertyName("proportion")]
    public required decimal Proportion { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("resultingPrice")]
    public required decimal ResultingPrice { get; init; }

    public static MarkupResult FromProportion(decimal cost, decimal proportion)
    {
        var markuped = cost * (1 + proportion);
        return new MarkupResult
        {
            Proportion = proportion,
            ResultingPrice = markuped,
            Amount = markuped - cost,
        };
    }
}
