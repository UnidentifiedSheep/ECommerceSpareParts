namespace Pricing.Application.Models.Pricing;

public sealed record MarkupResult
{
    public required decimal Proportion { get; init; }
    public required decimal AmountInBaseCurrency { get; init; }
    public required decimal PriceInBaseCurrency { get; init; }

    public static MarkupResult FromProportion(decimal cost, decimal proportion)
    {
        var markuped = cost * (1 + proportion);
        return new MarkupResult
        {
            Proportion = proportion,
            PriceInBaseCurrency = markuped,
            AmountInBaseCurrency = markuped - cost,
        };
    }
}