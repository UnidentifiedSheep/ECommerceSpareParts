using Mapster;

using ContractMetricPayload = Contracts.Models.Metric.MetricPayloadDto;
using MetricPayloadDto = Analytics.Abstractions.Dtos.CalculationJob.MetricPayloadDto;

namespace Analytics.Application.Configs.Mapster;

public static class MetricMapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<MetricPayloadDto, ContractMetricPayload>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.RangeEnd, s => s.RangeEnd)
            .Map(d => d.RangeStart, s => s.RangeStart)
            .Map(d => d.CurrencyId, s => s.CurrencyId);
        
        TypeAdapterConfig<ContractMetricPayload, MetricPayloadDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.RangeEnd, s => s.RangeEnd)
            .Map(d => d.RangeStart, s => s.RangeStart)
            .Map(d => d.CurrencyId, s => s.CurrencyId);
    }
}