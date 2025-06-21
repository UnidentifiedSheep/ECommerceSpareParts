using IntervalMap.Models;

namespace IntervalMap.IntervalVariations;

/// <summary>
/// NOT FOR USE.
/// BAD PERFORMANCE
/// </summary>
public class Interval5BitPacked<T> : IntervalMap<Interval<T>> where T : class
{
     private readonly byte[] _map;
     public sealed override double MaxValue { get; protected set; }
    public Interval5BitPacked(double maxValue)
    {
        if (maxValue < 1) maxValue = 1;
        MaxValue = maxValue;
        int scaledMaxValue = (int)(maxValue * ScaleFactor);
        _map = new byte[(scaledMaxValue + 1) * 5 / 8 + 1];
        Intervals.Add(new Interval<T>(0, 0));
    }


    public override IntervalMap<Interval<T>> AddInterval(Interval<T> interval)
    {
        if (!CheckIfIntervalValid(interval.Start, interval.End))
            throw new ArgumentException("Неверный интервал");
        if (Intervals.Count >= 32)
            throw new InvalidOperationException("Количество интервалов превышает 32 (максимум для 5-битных индексов).\n");

        int index = Intervals.Count;
        Intervals.Add(interval);

        int scaledStart = Scale(interval.Start);
        int scaledEnd = Scale(interval.End);

        for (int i = scaledStart; i <= scaledEnd; i++)
        {
            int bitIndex = i * 5;
            int byteIndex = bitIndex / 8;
            int bitOffset = bitIndex % 8;

            if (byteIndex >= _map.Length)
                throw new IndexOutOfRangeException("Interval exceeds map boundaries.");

            _map[byteIndex] |= (byte)(index << bitOffset);

            if (bitOffset > 3 && byteIndex + 1 < _map.Length)
                _map[byteIndex + 1] |= (byte)(index >> (8 - bitOffset));
            
        }

        return this;
    }
    
    public override bool RemoveInterval(Interval<T> interval)
    {
        var foundInterval = Intervals.FirstOrDefault(x => x.Equals(interval));
        if (foundInterval == null) return false;
        
        int scaledStart = Scale(interval.Start);
        int scaledEnd = Scale(interval.End);

        for (int i = scaledStart; i <= scaledEnd; i++)
        {
            int bitIndex = i * 5;
            int byteIndex = bitIndex / 8;
            int bitOffset = bitIndex % 8;

            if (byteIndex >= _map.Length)
                continue;

            byte mask = (byte)(~(0b11111 << bitOffset));
            _map[byteIndex] &= mask;

            if (bitOffset > 3 && byteIndex + 1 < _map.Length)
            {
                byte maskNext = (byte)(~(0b11111 >> (8 - bitOffset)));
                _map[byteIndex + 1] &= maskNext;
            }
        }

        Intervals.Remove(foundInterval);
        return true;
    }

    public override bool Contains(double value)
    {
        int scaledValue = Scale(value);
        int bitIndex = scaledValue * 5;
        int byteIndex = bitIndex / 8;
        int bitOffset = bitIndex % 8;

        if (byteIndex >= _map.Length)
            return false;

        int index = (_map[byteIndex] >> bitOffset) & 0x1F;

        if (bitOffset > 3 && byteIndex + 1 < _map.Length)
            index |= (_map[byteIndex + 1] << (8 - bitOffset)) & 0x1F;
        

        return index != 0;
    }

    public override Interval<T>? GetInterval(double value)
    {
        int scaledValue = Scale(value);
        int bitIndex = scaledValue * 5;
        int byteIndex = bitIndex / 8;
        int bitOffset = bitIndex % 8;

        if (byteIndex >= _map.Length)
            return null;

        int index = (_map[byteIndex] >> bitOffset) & 0x1F;

        if (bitOffset > 3 && byteIndex + 1 < _map.Length)
            index |= (_map[byteIndex + 1] << (8 - bitOffset)) & 0x1F;
        
        return index == 0 ? null : Intervals[index];
    }

    private bool CheckIfIntervalValid(double start, double end)
    {
        int scaledStart = Scale(start);
        int scaledEnd = Scale(end);

        if (scaledStart < 0 || scaledEnd * 5 / 8 >= _map.Length || scaledStart > scaledEnd)
            return false;

        return true;
    }

    public override bool IsIntersectionExists(Interval<T> interval)
    {
        int scaledStart = Scale(interval.Start);
        int scaledEnd = Scale(interval.End);

        for (int i = scaledStart; i <= scaledEnd; i++)
        {
            int bitIndex = i * 5;
            int byteIndex = bitIndex / 8;
            int bitOffset = bitIndex % 8;

            if (byteIndex >= _map.Length)
                continue;

            int index = (_map[byteIndex] >> bitOffset) & 0x1F;

            if (bitOffset > 3 && byteIndex + 1 < _map.Length)
                index |= (_map[byteIndex + 1] << (8 - bitOffset)) & 0x1F;

            if (index != 0) return true;
        }

        return false;
    }
}