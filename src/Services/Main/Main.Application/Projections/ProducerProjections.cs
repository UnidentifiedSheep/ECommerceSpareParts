using System.Linq.Expressions;
using Main.Application.Dtos.Producer;
using Main.Application.Dtos.Producer.Aliases;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Entities.Producer;

namespace Main.Application.Projections;

public static class ProducerProjections
{
    public static readonly Expression<Func<Producer, ProducerDto>> ToDto =
        x => new ProducerDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        };

    public static readonly Expression<Func<Producer, ProducerFullDto>> ToFullDto =
        x => new ProducerFullDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Aliases = x.Aliases.Select(z => z.Alias)
        };

    public static readonly Expression<Func<ProducerAlias, ProducerAliasDto>> ToAliasDto =
        x => new ProducerAliasDto
        {
            ProducerId = x.ProducerId,
            Alias = x.Alias
        };

    public static readonly Expression<Func<ProducerSupplierMapping, ProducerSupplierMappingDto>> ToSupplierMappingDto =
        x => new ProducerSupplierMappingDto
        {
            Id = x.Id,
            ProducerId = x.ProducerId,
            Supplier = x.Supplier,
            SupplierProducerName = x.SupplierProducerName
        };
}