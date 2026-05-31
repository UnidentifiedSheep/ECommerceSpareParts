using System.Linq.Expressions;
using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities;
using LinqKit;

namespace Analytics.Application.Handlers.Projections;

public static class MetricCalculationJobProjection
{
    public static readonly Expression<Func<MetricCalculationJob, CalculationJobDto>> ToDto =
        x => new CalculationJobDto
        {
            RequestId = x.RequestId,
            MetricSystemName = x.MetricSystemName,
            MetricId = x.MetricId,
            UpdateAt = x.UpdatedAt,
            CreateAt = x.CreatedAt,
            Status = x.Status,
            ErrorMessage = x.ErrorMessage
        };

    public static readonly Expression<Func<MetricCalculationJob?, CalculationJobDto?>> ToDtoOrDefault =
        x => x == null 
            ? null 
            : ToDto.Invoke(x);
}