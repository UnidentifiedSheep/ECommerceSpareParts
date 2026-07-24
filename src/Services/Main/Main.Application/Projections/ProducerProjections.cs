using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Main.Application.Dtos.Producer;
using Main.Application.Dtos.Producer.Aliases;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Entities.Producer;

namespace Main.Application.Projections;

public sealed class ProducerDtoProjectionProvider
    : ISingletonProjectionProvider<Producer, ProducerDto>
{
    public Expression<Func<Producer, ProducerDto>> Projection { get; } =
        x => new ProducerDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        };
}

public sealed class ProducerFullDtoProjectionProvider
    : ISingletonProjectionProvider<Producer, ProducerFullDto>
{
    public Expression<Func<Producer, ProducerFullDto>> Projection { get; } =
        x => new ProducerFullDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Aliases = x.Aliases.Select(z => z.Alias)
        };
}

public sealed class ProducerAliasDtoProjectionProvider
    : ISingletonProjectionProvider<ProducerAlias, ProducerAliasDto>
{
    public Expression<Func<ProducerAlias, ProducerAliasDto>> Projection { get; } =
        x => new ProducerAliasDto
        {
            ProducerId = x.ProducerId,
            Alias = x.Alias
        };
}

public sealed class ProducerSupplierMappingDtoProjectionProvider
    : ISingletonProjectionProvider<ProducerSupplierMapping, ProducerSupplierMappingDto>
{
    public Expression<Func<ProducerSupplierMapping, ProducerSupplierMappingDto>> Projection { get; } =
        x => new ProducerSupplierMappingDto
        {
            Id = x.Id,
            ProducerId = x.ProducerId,
            Supplier = x.Supplier,
            SupplierProducerName = x.SupplierProducerName
        };
}
