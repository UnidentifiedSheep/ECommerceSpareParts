using System.Linq.Expressions;
using ContractMetricPayload = Contracts.Models.Metric.MetricPayloadDto;
using MetricPayloadDto = Analytics.Application.Dtos.CalculationJob.MetricPayloadDto;

namespace Analytics.Application.Handlers.Projections;

public static class MetricPayloadProjection
{
    public static readonly Expression<Func<MetricPayloadDto, ContractMetricPayload>> ToContract =
        x => new ContractMetricPayload
        {
            CurrencyId = x.CurrencyId,
            RangeStart = x.RangeStart,
            RangeEnd = x.RangeEnd,
            ProductId = x.ProductId
        };

    public static readonly Expression<Func<ContractMetricPayload, MetricPayloadDto>> FromContract =
        x => new MetricPayloadDto
        {
            CurrencyId = x.CurrencyId,
            RangeStart = x.RangeStart,
            RangeEnd = x.RangeEnd,
            ProductId = x.ProductId
        };
}