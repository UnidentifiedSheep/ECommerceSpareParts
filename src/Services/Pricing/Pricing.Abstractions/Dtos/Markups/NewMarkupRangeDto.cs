namespace Pricing.Abstractions.Dtos.Markups;

public class NewMarkupRangeDto
{
    private double _rangeEnd;
    private double _rangeStart;

    public double RangeStart
    {
        get => _rangeStart;
        set => _rangeStart = Math.Round(value, 2);
    }

    public double RangeEnd
    {
        get => _rangeEnd;
        set => _rangeEnd = Math.Round(value, 2);
    }

    public decimal MarkupRate { get; set; }
}