using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

public abstract class Metric
{
    public Guid Id { get; set; }

    public int CurrencyId { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime RangeStart { get; set; }

    public DateTime RangeEnd { get; set; }

    public DateTime? RecalculatedAt { get; set; }

    public string Discriminator { get; set; } = null!;

    public RecalculationTags Tags { get; set; }

    public string DimensionKey
    {
        get;
        set
        {
            field = value;
            DimensionHash = ComputeHash(value);
        }
    } = string.Empty;

    public byte[] DimensionHash { get; private set; } = [];

    public abstract DependsOn DependsOn { get; protected set; }

    public string? Json { get; protected set; }
    public virtual Currency Currency { get; set; } = null!;

    private static byte[] ComputeHash(string key)
    {
        var full = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return full[..16];
    }
}

public abstract class Metric<T> : Metric where T : class
{
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