using Analytics.Core.Entities;
using Mapster;

namespace Analytics.Application.Configs;

public static class Mapster
{
    public static void Configure()
    {
        TypeAdapterConfig<global::Core.Models.Currency, Currency>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ToUsd, _ => 0);
    }
}