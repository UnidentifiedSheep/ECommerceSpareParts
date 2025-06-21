using IntervalMap.Models;

namespace IntervalMap.IntervalVariations;

public class IntervalMapByte<T> : IntervalMap<Interval<T>> where T : class 
{
    private readonly byte[] _map;
    public sealed override double MaxValue { get; protected set; }

    /// <summary>
    /// Инициализация карты, и ее размерности.
    /// </summary>
    /// <param name="maxValue">Максимальное значение в массиве, используется вычисления размерности массива</param>
    public IntervalMapByte(double maxValue)
    {
        MaxValue = maxValue;
        int scaledMaxValue = (int)(maxValue * ScaleFactor);
        _map = new byte[scaledMaxValue + 1];
        Intervals.Add(new Interval<T>(0,0));
    }

    public override IntervalMap<Interval<T>> AddInterval(Interval<T> interval)
    {
        if (!CheckIfIntervalValid(interval.Start, interval.End))
            throw new ArgumentException("Неверный интервал");
        if (Intervals.Count >= 256)
            throw new Exception($"Данная карта поддерживает максимум 255 интервалов. Текущее количество {Intervals.Count - 1}");
        
        int scaledStart = Scale(interval.Start);
        int scaledEnd = Scale(interval.End);

        
        int intervalIndex = Intervals.Count;
        Intervals.Add(interval);
        
        for (int i = scaledStart; i <= scaledEnd; i++)
            _map[i] = (byte)intervalIndex;
        return this;
    }
    
    public override bool RemoveInterval(Interval<T> interval)
    {
        var foundInterval = Intervals.FirstOrDefault(x => x.Equals(interval));
        if (foundInterval == null) return false;
        
        for (int i = Scale(foundInterval.Start); i <= Scale(foundInterval.End); i++)
            _map[i] = 0;
        
        Intervals.Remove(foundInterval);
        return true;
    }
    
    public override bool Contains(double value)
    {
        int scaledValue = Scale(value);
        if (scaledValue < 0 || scaledValue >= _map.Length)
            return false;

        return _map[scaledValue] != 0;
    }
    public override Interval<T>? GetInterval(double value)
    {
        int scaledValue = Scale(value);
        if (scaledValue < 0 || scaledValue >= _map.Length || _map[scaledValue] == 0)
            return null;
        
        return Intervals[_map[scaledValue]];
    }
    
    private bool CheckIfIntervalValid(double start, double end)
    {
        int scaledStart = Scale(start);
        int scaledEnd = Scale(end);

        if (scaledStart < 0 || scaledEnd >= _map.Length || scaledStart > scaledEnd)
            return false;
        
        return true;
    }
    
    public override bool IsIntersectionExists(Interval<T> interval)
    {
        int scaledStart = Scale(interval.Start);
        int scaledEnd = Scale(interval.End);

        for (int i = scaledStart; i <= scaledEnd; i++)
        {
            if (i > _map.Length - 1) break;
            if (_map[i] != 0) return true;
        }
        
        return false;
    }
}