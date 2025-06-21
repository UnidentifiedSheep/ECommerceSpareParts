using System.Data;
using IntervalMap.Enums;
using IntervalMap.Models;

namespace IntervalMap.IntervalVariations;

public class IntervalMapUShort<T> : IntervalMap<Interval<T>> where T : class
{
    private readonly ushort[] _map;
    public sealed override double MaxValue { get; protected set; }

    public IntervalMapUShort(double maxValue)
    {
        MaxValue = maxValue;
        _map = new ushort[Scale(maxValue) + 1];
        Array.Fill(_map, (ushort)0); 
        Intervals.Add(new Interval<T>(0, 0));
    }

    public override IntervalMap<Interval<T>> AddInterval(Interval<T> interval)
    {
        if (!CheckIfIntervalValid(interval.Start, interval.End))
            throw new ArgumentException("Неверный интервал"); 
        if (Intervals.Count >= ushort.MaxValue + 1)
            throw new InvalidOperationException("Количество интервалов превышает 65535 (максимум для ushort индексов).");
        
        int scaledStart = Scale(interval.Start);
        int scaledEnd = Scale(interval.End);
        
        ushort intervalIndex = (ushort)Intervals.Count;
        Intervals.Add(interval);
        
        for (int i = scaledStart; i <= scaledEnd; i++)
            _map[i] = intervalIndex;
        return this;
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
        if (scaledValue < 0 || scaledValue >= _map.Length || _map[scaledValue] == 0 || !CheckAfterComa(value))
            return null;
        
        
        return Intervals[_map[scaledValue]];
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

    private bool CheckIfIntervalValid(double start, double end)
    {
        int scaledStart = Scale(start);
        int scaledEnd = Scale(end);

        if (scaledStart < 0 || scaledEnd >= _map.Length || scaledStart > scaledEnd)
            return false;
        
        return true;
    }
    
}