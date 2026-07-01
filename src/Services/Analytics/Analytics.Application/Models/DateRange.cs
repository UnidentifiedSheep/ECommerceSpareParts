namespace Analytics.Application.Models;

public record DateRange
{
    public DateRange(DateTime from, DateTime to)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(from, to);
        From = from;
        To = to;
    }

    public DateTime From { get; }
    public DateTime To { get; }
}