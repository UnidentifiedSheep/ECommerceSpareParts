using IntervalMap.Enums;
using IntervalMap.Models;

namespace IntervalMap.IntervalVariations;

/// <summary>
/// Создает адаптивный битмап, который увеличивается либо уменьшается в зависимости от максимального размера, и количества интервалов.
/// </summary>
/// <param name="useExperimental">Если true, то будут использоваться упакованные битмапы, такие как 4BitPacked или 5BitPacked.</param>
public class AdaptiveIntervalMap<T>(bool useExperimental = false, Intersection intersection = Intersection.CanNotIntersect) : IntervalMap<Interval<T>> where T : class
{
    public bool CanIntersect { get; } = intersection == Intersection.CanIntersect;
    public bool UseExperimental { get; } = useExperimental;
    public sealed override double MaxValue { get; protected set; } = 0;
    private IntervalMap<Interval<T>> _currentMap = useExperimental ? new Interval4BitPacked<T>(0) : new IntervalMapByte<T>(0);
    public override IntervalMap<Interval<T>> AddInterval(Interval<T> interval)
    {
        if (interval.End <= _currentMap.MaxValue)
            _currentMap.AddInterval(interval);
        else
            IncreaseAndAddInterval(interval);
        Intervals.Add(interval);
        return this;
    }

    private void IncreaseAndAddInterval(Interval<T> interval)
    {
        if (!CanIntersect && IsIntersectionExists(interval))
            throw new InvalidOperationException("Интервал соприкасается с другим интервалом.");
        MaxValue = Math.Max(MaxValue, interval.End); 
        var newInterval = GetCorrectIntervalMap(Intervals.Count + 1, MaxValue);
        foreach (var inter in Intervals)
            newInterval.AddInterval(inter);
        _currentMap = newInterval;
        _currentMap.AddInterval(interval);
    }

    private IntervalMap<Interval<T>> GetCorrectIntervalMap(int intervalCount, double maxValue)
    {
        return intervalCount switch
        {
            < 16 when UseExperimental => new Interval4BitPacked<T>(maxValue),
            < 32 when UseExperimental => new Interval5BitPacked<T>(maxValue),
            < 255  => new IntervalMapByte<T>(maxValue),
            < ushort.MaxValue => new IntervalMapUShort<T>(maxValue),
            < int.MaxValue => new IntervalMapInt<T>(maxValue),
            _ => throw new ArgumentOutOfRangeException(nameof(intervalCount),
                $"Интервала со значение более чем {int.MaxValue} не допустим.")
        };
    }
    public override bool Contains(double value) => _currentMap.Contains(value);

    public override Interval<T>? GetInterval(double value) => _currentMap.GetInterval(value);

    public override bool RemoveInterval(Interval<T> interval)
    {
        var foundInterval = Intervals.FirstOrDefault(x => x.Equals(interval));
        if (foundInterval == null) return false;
        bool idERemoved = _currentMap.RemoveInterval(foundInterval);
        if (idERemoved) Intervals.Remove(interval);
        return true;
    }

    public override bool IsIntersectionExists(Interval<T> interval) => _currentMap.IsIntersectionExists(interval);
    
}