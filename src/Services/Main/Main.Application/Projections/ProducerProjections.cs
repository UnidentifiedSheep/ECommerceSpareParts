using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Application.Dtos.Producer;
using Main.Application.Dtos.Producer.Aliases;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Entities.Producer;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class ProducerDtoProjectionProvider
    : ProjectionProviderBase<Producer, ProducerDto>
{
    public override Expression<Func<Producer, ProducerDto>> Projection { get; } =
        x => new ProducerDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        };
}

[Lifetime(Lifetime.Singleton)]
public sealed class ProducerFullDtoProjectionProvider
    : ProjectionProviderBase<Producer, ProducerFullDto>
{
    public override Expression<Func<Producer, ProducerFullDto>> Projection { get; } =
        x => new ProducerFullDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Aliases = x.Aliases.Select(z => z.Alias)
        };
}

[Lifetime(Lifetime.Singleton)]
public sealed class ProducerAliasDtoProjectionProvider
    : ProjectionProviderBase<ProducerAlias, ProducerAliasDto>
{
    public override Expression<Func<ProducerAlias, ProducerAliasDto>> Projection { get; } =
        x => new ProducerAliasDto
        {
            ProducerId = x.ProducerId,
            Alias = x.Alias
        };
}

[Lifetime(Lifetime.Singleton)]
public sealed class ProducerSupplierMappingDtoProjectionProvider
    : ProjectionProviderBase<ProducerSupplierMapping, ProducerSupplierMappingDto>
{
    public override Expression<Func<ProducerSupplierMapping, ProducerSupplierMappingDto>> Projection { get; } =
        x => new ProducerSupplierMappingDto
        {
            Id = x.Id,
            ProducerId = x.ProducerId,
            Supplier = x.Supplier,
            SupplierProducerName = x.SupplierProducerName
        };
}
