namespace Pricing.Application.Models.Pricing;

public sealed record MarkupResult
{
    public required decimal Proportion { get; init; }
    public required decimal Amount { get; init; }
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