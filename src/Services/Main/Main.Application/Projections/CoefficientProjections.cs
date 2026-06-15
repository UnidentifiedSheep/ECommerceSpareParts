using System.Linq.Expressions;
using Main.Application.Dtos.Coefficient;
using Main.Entities;

namespace Main.Application.Projections;

public static class CoefficientProjections
{
    public static readonly Expression<Func<Coefficient, CoefficientDto>> ToDto =
        x => new CoefficientDto
        {
            Name = x.Name,
            Value = x.Value,
            Type = x.Type
        };
}