namespace Analytics.Application.NamedObjects.Analyzers.Markup;

public sealed class MarkupBucketBuilder
{
    private double _m2;
    public decimal FromCost { get; private set; }
    public decimal ToCost { get; private set; }

    public int Count { get; private set; }

    public double Mean { get; private set; }

    public double StdDev => Count > 1 ? Math.Sqrt(_m2 / (Count - 1)) : 0;

    public void Add(decimal cost, decimal markup)
    {
        if (Count == 0) FromCost = cost;

        ToCost = cost;
        Count++;

        var x = (double)markup;
        var delta = x - Mean;
        Mean += delta / Count;
        var delta2 = x - Mean;
        _m2 += delta * delta2;
    }

    public MarkupBucketBuilder Copy()
    {
        return new MarkupBucketBuilder
        {
            FromCost = FromCost,
            ToCost = ToCost,
            Count = Count,
            Mean = Mean,
            _m2 = _m2
        };
    }

    public MarkupRangeDraft Build()
    {
        return new MarkupRangeDraft(
            FromCost,
            ToCost,
            (decimal)Mean,
            (decimal)StdDev,
            Count);
    }
}