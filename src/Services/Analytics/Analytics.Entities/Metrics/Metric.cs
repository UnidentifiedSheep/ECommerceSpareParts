using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Analytics.Enums;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Analytics.Entities.Metrics;

public abstract class Metric : AuditableEntity<Metric, Guid>, ILinqEntity<Metric, Guid>
{
    private readonly List<MetricJob> _jobs = [];
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
    public IReadOnlyCollection<MetricJob> Jobs => _jobs;
    public abstract Type DataType { get; }

    public void ConfigurePeriod(
        int currencyId,
        DateTime rangeStart,
        DateTime rangeEnd)
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

    public void AddJob(Guid jobId) { _jobs.Add(MetricJob.Create(Id, jobId)); }

    public void MarkDirty() { Tags |= RecalculationTags.RecalculationNeeded; }

    public void Disable() { Tags |= RecalculationTags.Disabled; }

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

    private byte[] ComputeNaturalKey() { return GetNaturalKey(this); }

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
        start = start.ToUniversalTime();
        end = end.ToUniversalTime();

        var key = string.Create(
            CultureInfo.InvariantCulture,
            $"{start:yyyy-MM-ddTHH:mm:ssZ}|{end:yyyy-MM-ddTHH:mm:ssZ}|{discriminator}|{dimensionKey}|{currencyId}");

        return SHA256.HashData(
            Encoding.UTF8
                .GetBytes(key));
    }

    public override Guid GetId() { return Id; }

    public abstract object? GetData();
    public static Expression<Func<Metric, Guid>> GetKeySelector() => x => x.Id;
    public static Expression<Func<Metric, bool>> GetEqualityExpression(Guid key) => x =>  x.Id == key;
}

public abstract class Metric<T> : Metric where T : class
{
    public override Type DataType => typeof(T);

    [NotMapped]
    public T? Data
    {
        get
        {
            field ??= Json == null ? null : JsonSerializer.Deserialize<T>(Json);
            return field;
        }
        private set
        {
            field = value;
            Json = value == null ? null : JsonSerializer.Serialize(value);
        }
    }

    public void SetData(T? data) { Data = data; }

    public override object? GetData() { return Data; }
}