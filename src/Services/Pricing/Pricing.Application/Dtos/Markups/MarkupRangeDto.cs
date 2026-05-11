namespace Pricing.Abstractions.Dtos.Markups;

public class MarkupRangeDto
{
    public int Id { get; set; }

    public decimal RangeStart { get; set; }

    public decimal RangeEnd { get; set; }

    public decimal MarkupRate { get; set; }
}