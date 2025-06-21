using IntervalMap.Models;

namespace IntervalMap.IntervalVariations;

/// <summary>
/// NOT FOR USE.
/// BAD PERFORMANCE
/// </summary>
public class Interval4BitPacked<T> : IntervalMap<Interval<T>> where T : class
{
    public sealed override double MaxValue { get; protected set; }
    private readonly byte[] _map;
    public Interval4BitPacked(double maxValue)
    {
        if (maxValue < 1) maxValue = 1;
        MaxValue = maxValue;
        int scaledMaxValue = (int)(maxValue * ScaleFactor);
        _map = new byte[(scaledMaxValue + 2) / 2];
        Intervals.Add(new Interval<T>(0, 0));
    }
    
    public override IntervalMap<Interval<T>> AddInterval(Interval<T> interval)
    {
        if (!CheckIfIntervalValid(interval.Start, interval.End))
            throw new ArgumentException("Неверный интервал");
        if (Intervals.Count >= 16)
            throw new InvalidOperationException("Количество интервалов превышает 16 (максимум для 4-битных индексов).");

        int index = Intervals.Count;
        Intervals.Add(interval);
        
        int scaledStart = Scale(interval.Start);
        int scaledEnd = Scale(interval.End);
        
        for (int i = scaledStart; i <= scaledEnd; i++)
        {
            int byteIndex = i / 2; 
            int isSecond = i % 2; 

            if (byteIndex >= _map.Length)
                throw new IndexOutOfRangeException("Interval exceeds map boundaries.");

            if (isSecond == 0)
                _map[byteIndex] = (byte)((_map[byteIndex] & 0xF0) | index);
            else
                _map[byteIndex] = (byte)((_map[byteIndex] & 0x0F) | (index << 4));
        }

        return this;
    }

    public override bool Contains(double value)
    {
        int scaledValue = (int)(value * 100);
        int byteIndex = scaledValue / 2;
        int isSecond = scaledValue % 2;

        if (byteIndex >= _map.Length)
            return false;

        int index = isSecond == 0
            ? _map[byteIndex] & 0x0F 
            : (_map[byteIndex] >> 4) & 0x0F;

        return index != 0;
    }

    public override Interval<T>? GetInterval(double value)
    {
        int scaledValue = Scale(value);
        int byteIndex = scaledValue / 2;
        int isSecond = scaledValue % 2;

        if (byteIndex >= _map.Length)
            return null;

        int index = isSecond == 0
            ? _map[byteIndex] & 0x0F 
            : (_map[byteIndex] >> 4) & 0x0F;

        return index == 0 ? null : Intervals[index];
    }
    
    public override bool RemoveInterval(Interval<T> interval)
    {
        var foundInterval = Intervals.FirstOrDefault(x => x.Equals(interval));
        if (foundInterval == null) return false;

        int scaledStart = Scale(interval.Start);
        int scaledEnd = Scale(interval.End);

        for (int i = scaledStart; i <= scaledEnd; i++)
        {
            int byteIndex = i / 2;
            int isSecond = i % 2;

            if (byteIndex >= _map.Length)
                continue;

            if (isSecond == 0)
                _map[byteIndex] &= 0xF0;
            else
                _map[byteIndex] &= 0x0F; 
            
        }

        Intervals.Remove(foundInterval);
        return true;
    }
    
    private bool CheckIfIntervalValid(double start, double end)
    {
        int scaledStart = Scale(start);
        int scaledEnd = Scale(end);

        if (scaledStart < 0 || scaledEnd/2 >= _map.Length || scaledStart > scaledEnd)
            return false;
        
        return true;
    }

    public override bool IsIntersectionExists(Interval<T> interval)
    {
        int scaledStart = Scale(interval.Start);
        int scaledEnd = Scale(interval.End);

        for (int i = scaledStart; i <= scaledEnd; i++)
        {
            int byteIndex = i / 2; 
            int isSecond = i % 2; 

            if (byteIndex >= _map.Length)
                return false;

            int index = isSecond == 0
                ? _map[byteIndex] & 0x0F  
                : (_map[byteIndex] >> 4) & 0x0F; 

            if (index != 0) 
                return true;
        }

        return false; 
    }
}