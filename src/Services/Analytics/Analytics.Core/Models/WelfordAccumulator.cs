namespace Analytics.Core.Models;

public class WelfordAccumulator
{
    public int Count { get; private set; } = 0;
    public double Mean { get; private set; } = 0.0;
    private double M2 = 0.0;

    public void Add(double x)
    {
        Count++;
        var delta = x - Mean;
        Mean += delta / Count;
        var delta2 = x - Mean;
        M2 += delta * delta2;
    }

    // population variance: M2 / Count
    public double VariancePopulation => Count > 0 ? M2 / Count : 0.0;
}