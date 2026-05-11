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

    public string DimensionKey
    {
        get;
        private set
        {
            field = value;
            DimensionHash = ComputeHash(value);
        }
    } = string.Empty;

    public byte[] DimensionHash { get; private set; } = [];

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
    }

    private static byte[] ComputeHash(string key)
    {
        var full = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return full[..16];
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