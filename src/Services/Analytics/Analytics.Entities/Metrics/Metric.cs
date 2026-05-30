using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Analytics.Enums;
using Domain;
using Domain.Extensions;

namespace Analytics.Entities.Metrics;

public abstract class Metric : AuditableEntity<Metric, Guid>
{
    public Guid Id { get; private set; }

    public int CurrencyId { get; private set; }

    public DateTime RangeStart { get; private set; }

    public DateTime RangeEnd { get; private set; }

    public DateTime? RecalculatedAt { get; private set; }

    public string Discriminator { get; private set; } = null!;

    public RecalculationTags Tags { get; private set; }

    public string DimensionKey { get; private set; } = string.Empty;
    public byte[] NaturalKey { get; private set; } = [];

    public abstract DependsOn DependsOn { get; protected set; }

    public string? Json { get; protected set; }

    public void ConfigurePeriod(int currencyId, DateTime rangeStart, DateTime rangeEnd)
    {
        currencyId.AgainstLessOrEqual(
            0,
            () => new ArgumentException("Currency id must be greater than zero.", nameof(currencyId)));

        if (rangeStart > rangeEnd)
            throw new ArgumentException("Range start must be less than or equal to range end.");

        CurrencyId = currencyId;
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
        NaturalKey = ComputeNaturalKey();
        MarkDirty();
    }

    public void MarkDirty()
    {
        Tags |= RecalculationTags.RecalculationNeeded;
    }

    public void CompleteRecalculation()
    {
        RecalculatedAt = DateTime.UtcNow;
        Tags &= ~RecalculationTags.RecalculationNeeded;
    }

    protected void SetDimensionKey(string dimensionKey)
    {
        DimensionKey = dimensionKey
            .TrimSafe()
            .AgainstNullOrWhiteSpace("metric.dimension.key.required")
            .AgainstTooLong(200, "metric.dimension.key.too.long");

        NaturalKey = ComputeNaturalKey();
    }

    private byte[] ComputeNaturalKey()
    {
        return GetNaturalKey(this);
    }

    public static byte[] GetNaturalKey(Metric metric)
    {
        return GetNaturalKey(
            metric.RangeStart, 
            metric.RangeEnd, 
            metric.GetType().Name,
            metric.DimensionKey, 
            metric.CurrencyId);
    }

    public static byte[] GetNaturalKey(
        DateTime start,
        DateTime end,
        string discriminator,
        string dimensionKey,
        int currencyId)
    {
        start = ToUtc(start);
        end = ToUtc(end);

        return SHA256.HashData(
            Encoding.UTF8
                .GetBytes($"{start:O}|{end:O}|{discriminator}|{dimensionKey}|{currencyId}"));
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public override Guid GetId()
    {
        return Id;
    }
}

public abstract class Metric<T> : Metric where T : class
{
    [NotMapped]
    public T? Data
    {
        get;
        private set
        {
            field = value;
            Json = value == null ? null : JsonSerializer.Serialize(value);
        }
    }

    public void SetData(T? data)
    {
        Data = data;
    }
}
