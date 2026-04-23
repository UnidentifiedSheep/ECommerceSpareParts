using System.Linq.Expressions;
using Main.Application.Dtos.Amw.Coefficients;
using Main.Entities;

namespace Main.Application.Handlers.Projections;

public static class CoefficientProjections
{
    public static Expression<Func<Coefficient, CoefficientDto>> ToDto =
        x => new CoefficientDto
        {
            Name = x.Name,
            Value = x.Value,
            Type = x.Type,
        };
}