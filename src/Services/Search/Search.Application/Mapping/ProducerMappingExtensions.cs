using Search.Application.Dtos.Producers;
using Search.Entities;

namespace Search.Application.Mapping;

public static class ProducerMappingExtensions
{
    public static ProducerSearchDto ToProducerSearchDto(this Producer producer)
        => new()
        {
            Id = producer.Id,
            Name = producer.Name,
            Description = producer.Description
        };

    public static ProducerDto ToProducerDto(this Producer producer)
        => new()
        {
            Id = producer.Id,
            Name = producer.Name,
            Description = producer.Description,
            OtherNames = producer.OtherNames.Select(x => x.ToProducerOtherNameDto())
        };

    public static ProducerOtherNameDto ToProducerOtherNameDto(this ProducerOtherName otherName)
        => new()
        {
            OtherName = otherName.OtherName,
            WhereUsed = otherName.WhereUsed
        };
}
