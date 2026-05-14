using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Producer;

public class ProducerOtherName : Entity<ProducerOtherName, ProducerOtherNameKey>
{
    private ProducerOtherName()
    {
    }

    private ProducerOtherName(int producerId, string otherName, string whereUsed)
    {
        ProducerId = producerId;
        SetOtherName(otherName);
        SetWhereUsed(whereUsed);
    }

    [ValidateTuple("PK")]
    public int ProducerId { get; }

    [ValidateTuple("PK")]
    public string OtherName { get; private set; } = null!;

    [ValidateTuple("PK")]
    public string WhereUsed { get; private set; } = null!;

    public static ProducerOtherName Create(int producerId, string otherName, string whereUsed)
    {
        return new ProducerOtherName(producerId, otherName, whereUsed);
    }

    public void SetOtherName(string otherName)
    {
        OtherName = Producer.ToNormalizedName(otherName);
    }

    public void SetWhereUsed(string whereUsed)
    {
        whereUsed = whereUsed.Trim()
            .AgainstTooLong(64, "producer.other.name.where.used.max.length")
            .AgainstTooShort(2, "producer.other.name.where.used.min.length");

        WhereUsed = whereUsed.ToUpperInvariant();
    }

    public override ProducerOtherNameKey GetId()
    {
        return new ProducerOtherNameKey(ProducerId, OtherName, WhereUsed);
    }

    public override Expression<Func<ProducerOtherName, bool>> GetEqualityExpression(ProducerOtherNameKey key)
        => x => x.ProducerId == key.ProducerId && x.OtherName == key.OtherName && x.WhereUsed == key.WhereUsed;
}

public readonly struct ProducerOtherNameKey(int producerId, string otherName, string whereUsed) : ICompositeKey
{
    public int ProducerId => producerId;
    public string OtherName => otherName;
    public string WhereUsed => whereUsed;

    public object[] ToArray()
    {
        return [producerId, otherName, whereUsed];
    }
}