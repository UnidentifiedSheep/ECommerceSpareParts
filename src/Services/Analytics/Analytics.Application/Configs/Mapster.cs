using Analytics.Entities;
using Mapster;
using ContractCurrency = Contracts.Models.Currency.Currency;
namespace Analytics.Application.Configs;

public static class Mapster
{
    public static void Configure()
    {
        TypeAdapterConfig<ContractCurrency, Currency>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ToUsd, _ => 0);
    }
}