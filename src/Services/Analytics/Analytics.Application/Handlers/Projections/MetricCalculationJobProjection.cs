using System.Linq.Expressions;
using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Entities;

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
}