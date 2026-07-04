using Search.Application.Dtos.Producers;
using Search.Entities;
using ProducerAlias = Search.Application.Dtos.Producers.ProducerAlias;

namespace Search.Application.Mapping;

public static class ProducerMappingExtensions
{
    public static ProducerSearchDto ToProducerSearchDto(this Producer producer)
    {
        return new ProducerSearchDto
        {
            Id = producer.Id,
            Name = producer.Name,
            Description = producer.Description
        };
    }

    public static ProducerDto ToProducerDto(this Producer producer)
    {
        return new ProducerDto
        {
            Id = producer.Id,
            Name = producer.Name,
            Description = producer.Description,
            Aliases = producer.Aliases.Select(x => x.ToProducerAliasDto())
        };
    }

    public static ProducerAlias ToProducerAliasDto(this Entities.ProducerAlias alias)
    {
        return new ProducerAlias
        {
            Alias = alias.Alias
        };
    }
}