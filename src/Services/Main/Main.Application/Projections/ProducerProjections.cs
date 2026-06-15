using System.Linq.Expressions;
using Main.Application.Dtos.Producer;
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
    
    public static readonly Expression<Func<ProducerOtherName, ProducerOtherNameDto>> ToOtherNameDto =
        x => new ProducerOtherNameDto
        {
            ProducerId = x.ProducerId,
            OtherName = x.OtherName,
            WhereUsed = x.WhereUsed
        };
}