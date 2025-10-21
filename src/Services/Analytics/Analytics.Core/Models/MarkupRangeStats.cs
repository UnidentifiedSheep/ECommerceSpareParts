namespace Analytics.Core.Models;

public class MarkupRangeStats
{
    public decimal From { get; set; }
    public decimal To { get; set; }
    public decimal Mean { get; set; }
    public decimal StdDev { get; set; }
    public int Count { get; set; }
}