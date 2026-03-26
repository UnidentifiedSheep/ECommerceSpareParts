using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

public abstract class Metric
{
    protected Metric()
    {
        DimensionKey = string.Empty;
        DimensionHash = [];
    }

    protected Metric(string dimensionKey = "")
    {
        DimensionKey = dimensionKey;
        DimensionHash = ComputeHash(DimensionKey);
    }

    public Guid Id { get; set; }

    public int CurrencyId { get; protected set; }

    public Guid CreatedBy { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime RangeStart { get; protected set; }

    public DateTime RangeEnd { get; protected set; }

    public DateTime? RecalculatedAt { get; protected set; }

    public string Discriminator { get; protected set; } = null!;

    public RecalculationTags Tags { get; protected set; }

    public string DimensionKey { get; protected set; }

    public byte[] DimensionHash { get; protected set; }

    public abstract DependsOn DependsOn { get; protected set; }

    public string? Json { get; protected set; }
    public virtual Currency Currency { get; protected set; } = null!;

    public void SetCalculated()
    {
        RecalculatedAt = DateTime.UtcNow;
        Tags = RecalculationTags.None;
    }

    private static byte[] ComputeHash(string key)
    {
        var full = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return full[..16];
    }
}

public abstract class Metric<T> : Metric where T : class
{
    protected Metric()
    {
    }

    protected Metric(string dimensionKey = "") : base(dimensionKey)
    {
    }

    [NotMapped]
    public T? Data
    {
        get;
        set
        {
            field = value;
            Json = value == null ? null : JsonSerializer.Serialize(value);
        }
    }
}