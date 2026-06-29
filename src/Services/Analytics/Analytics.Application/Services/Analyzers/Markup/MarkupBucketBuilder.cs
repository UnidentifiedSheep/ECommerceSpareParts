namespace Analytics.Application.Services.Analyzers.Markup;

public sealed class MarkupBucketBuilder
{
    public decimal FromCost { get; private set; }
    public decimal ToCost { get; private set; }

    public int Count { get; private set; }

    private double _m2;

    public double StdDev => Count > 1 
        ? Math.Sqrt(_m2 / (Count - 1)) 
        : 0;

    public double Mean { get; private set; }

    public void Add(decimal cost, decimal markupPercent)
    {
        if (Count == 0)
            FromCost = cost;

        ToCost = cost;
        Count++;

        var x = (double)markupPercent;
        var delta = x - Mean;
        Mean += delta / Count;
        var delta2 = x - Mean;
        _m2 += delta * delta2;
    }
}