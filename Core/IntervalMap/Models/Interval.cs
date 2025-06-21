namespace IntervalMap.Models;

public class Interval<T> where T : class
{
    public double Start { get; private set; }
    public double End { get; private set; }
    public T? Value { get; private set; }

    public Interval(double start, double end, T? value = null)
    {
        if(start < 0 || end < 0)
            throw new ArgumentException("Значение интервала не может быть меньше нуля.");
        if ((Math.Round(start * 100, 4) % 1 != 0) || (Math.Round(end * 100, 4) % 1 != 0))
            throw new ArgumentException("У числа не должен быть более двух знаков после запятой.");
        Start = start;
        End = end;
        Value = value;
    }

    public bool IsNull() => Value == null;

    public override bool Equals(object? obj)
    {
        var other = (Interval?)obj;
        if(other == null) return false;
        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    protected bool Equals(Interval other) => Start.Equals(other.Start) && End.Equals(other.End);
    

    public override int GetHashCode() => HashCode.Combine(Start, End);
    
}
public class Interval
{
    public double Start { get; private set; }
    public double End { get; private set; }
    public Interval(double start, double end)
    {
        if(start < 0 || end < 0)
            throw new ArgumentException("Значение интервала не может быть меньше нуля.");
        if (((start * 100) % 1 != 0) || (end * 100 % 1 != 0))
            throw new ArgumentException("У числа не должен быть более двух знаков после запятой.");
        Start = start;
        End = end;
    }

    public override bool Equals(object? obj)
    {
        var other = (Interval?)obj;
        if(other == null) return false;
        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    protected bool Equals(Interval other)
    {
        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }
}