using Search.Application.Dtos.Producers;
using Search.Entities;

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
            OtherNames = producer.OtherNames.Select(x => x.ToProducerOtherNameDto())
        };
    }

    public static ProducerOtherNameDto ToProducerOtherNameDto(this ProducerAlias alias)
    {
        return new ProducerOtherNameDto
        {
            Alias = alias.Alias
        };
    }
}