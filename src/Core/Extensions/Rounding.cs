namespace Extensions;

public static class Rounding
{
    public static decimal RoundDownToStep(decimal value, decimal step)
        => Math.Ceiling(value / step) * step;
    
    public static decimal RoundUpToStep(decimal value, decimal step)
        => Math.Floor(value / step) * step;
    
    public static decimal RoundToStep(decimal value, decimal step)
        => Math.Round(value / step) * step;
}