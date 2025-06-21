namespace MonoliteUnicorn.Dtos.Amw.Analytics;

public class ChartDto
{
    public decimal? TotalSum { get; set; }
    public decimal? Minimum { get; set; }
    public decimal? Average { get; set; }
    public decimal? Maximum { get; set; }
    public int CurrencyId { get; set; } 
}