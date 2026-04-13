using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Main.Entities.Producer.ValueObjects;

namespace Main.Entities.Producer;

public class ProducerOtherName : Entity<ProducerOtherName, ProducerOtherNameKey>
{
    [ValidateTuple("PK")]
    public int ProducerId { get; private set; }

    [ValidateTuple("PK")]
    public Name OtherName { get; private set; } = null!;

    [ValidateTuple("PK")]
    public string WhereUsed { get; private set; } = null!;

    private ProducerOtherName() {}

    private ProducerOtherName(int producerId, Name otherName, string whereUsed)
    {
        ProducerId = producerId;
        OtherName = otherName;
        SetWhereUsed(whereUsed);
    }

    public static ProducerOtherName Create(int producerId, Name otherName, string whereUsed)
    {
        return new ProducerOtherName(producerId, otherName, whereUsed);
    }

    public void SetOtherName(Name otherName)
    {
        OtherName = otherName;
    }
    
    public void SetWhereUsed(string whereUsed)
    {
        whereUsed = whereUsed.Trim()
            .AgainstTooLong(64, "producer.other.name.where.used.max.length")
            .AgainstTooShort(2, "producer.other.name.where.used.min.length");
        
        WhereUsed = whereUsed.ToUpperInvariant();
    }
    
    public override ProducerOtherNameKey GetId() => new(ProducerId, OtherName, WhereUsed);
}

public readonly struct ProducerOtherNameKey(int producerId, string otherName, string whereUsed) : ICompositeKey
{
    public int ProducerId => producerId;
    public string OtherName => otherName;
    public string WhereUsed => whereUsed;
    
    public object[] ToArray() => [producerId, otherName, whereUsed];
}