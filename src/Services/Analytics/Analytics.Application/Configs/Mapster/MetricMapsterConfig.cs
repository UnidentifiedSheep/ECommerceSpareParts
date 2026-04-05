using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Entities;
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

        TypeAdapterConfig<MetricCalculationJob, CalculationJobDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.RequestId, s => s.RequestId)
            .Map(d => d.ErrorMessage, s => s.ErrorMessage)
            .Map(d => d.MetricSystemName, s => s.MetricSystemName)
            .Map(d => d.CreateAt, s => s.CreateAt)
            .Map(d => d.UpdateAt, s => s.UpdateAt)
            .Map(d => d.Status, s => s.Status)
            .Map(d => d.MetricId, s => s.MetricId);
    }
}