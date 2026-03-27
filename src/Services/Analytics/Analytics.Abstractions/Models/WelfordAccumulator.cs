namespace Analytics.Abstractions.Models;

public class WelfordAccumulator
{
    private double M2;
    public int Count { get; private set; }
    public double Mean { get; private set; }

    // population variance: M2 / Count
    public double VariancePopulation => Count > 0 ? M2 / Count : 0.0;

    public void Add(double x)
    {
        Count++;
        var delta = x - Mean;
        Mean += delta / Count;
        var delta2 = x - Mean;
        M2 += delta * delta2;
    }
}