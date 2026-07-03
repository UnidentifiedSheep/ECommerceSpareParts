using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Main.Entities.DomainEvents.Producer;

namespace Main.Entities.Producer;

public class ProducerAlias : Entity<ProducerAlias, string>,
    ILinqEntity<ProducerAlias, string>
{
    private ProducerAlias() { }

    private ProducerAlias(
        int producerId,
        string otherName)
    {
        ProducerId = producerId;
        SetAlias(otherName);
    }

    public int ProducerId { get; }

    [Validate]
    public string Alias { get; private set; } = null!;

    public static Expression<Func<ProducerAlias, string>> GetKeySelector() { return x => x.Alias; }

    public static Expression<Func<ProducerAlias, bool>> GetEqualityExpression(string key)
    {
        return x => x.Alias == key;
    }

    public static ProducerAlias Create(
        int producerId,
        string otherName)
    {
        return new ProducerAlias(
            producerId,
            otherName);
    }

    public override void OnDeleted() => AddDomainEvent(new ProducerAliasDeletedDomainEvent(ProducerId, Alias));
    public override void OnCreated() => AddDomainEvent(new ProducerAliasCreatedDomainEvent(ProducerId, Alias));

    private void SetAlias(string otherName) { Alias = Producer.ToNormalizedName(otherName); }

    public override string GetId() { return Alias; }
}