using Analytics.Entities;
using Mapster;
using ContractCurrency = Contracts.Models.Currency.Currency;

namespace Analytics.Application.Configs.Mapster;

public static class MapsterConfig
{
    public static void Configure()
    {
        PurchaseFactMapsterConfig.Configure();
        MetricMapsterConfig.Configure();

        TypeAdapterConfig<ContractCurrency, Currency>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ToUsd, _ => 0);
    }
}