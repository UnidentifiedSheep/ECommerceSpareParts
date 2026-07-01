using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Producer;

public class ProducerOtherName : Entity<ProducerOtherName, string>,
    ILinqEntity<ProducerOtherName, string>
{
    private ProducerOtherName() { }

    private ProducerOtherName(
        int producerId,
        string otherName,
        string? whereUsed)
    {
        ProducerId = producerId;
        SetOtherName(otherName);
        SetWhereUsed(whereUsed);
    }

    public int ProducerId { get; }

    [Validate]
    public string OtherName { get; private set; } = null!;

    public string WhereUsed { get; private set; } = null!; //TODO: should be removed.

    public static Expression<Func<ProducerOtherName, string>> GetKeySelector() { return x => x.OtherName; }

    public static Expression<Func<ProducerOtherName, bool>> GetEqualityExpression(string key)
    {
        return x => x.OtherName == key;
    }

    public static ProducerOtherName Create(
        int producerId,
        string otherName,
        string? whereUsed)
    {
        return new ProducerOtherName(
            producerId,
            otherName,
            whereUsed);
    }

    public void SetOtherName(string otherName) { OtherName = Producer.ToNormalizedName(otherName); }

    public void SetWhereUsed(string? whereUsed)
    {
        whereUsed = whereUsed.TrimSafe()
            .AgainstTooLong(64, "producer.other.name.where.used.max.length");

        WhereUsed = whereUsed.ToUpperInvariant();
    }

    public override string GetId() { return OtherName; }
}