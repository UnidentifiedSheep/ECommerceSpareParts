using Abstractions;
using Search.Entities;

namespace Search.Application.Configs;

public static class SortByConfig
{
    public static void Configure()
    {
        QueryableSortBy.Value
            .MapDefault<Product, int>(x => x.Id)
            .Map<Product, int>("id", x => x.Id)
            .Map<Product, string>("sku", x => x.NormalizedSku)
            .Map<Product, int>("producerId", x => x.ProducerId)
            .Map<Product, int>("stock", x => x.Stock)
            .Map<Product, decimal?>("length", x => x.Dimensions!.LengthM)
            .Map<Product, decimal?>("width", x => x.Dimensions!.WidthM)
            .Map<Product, decimal?>("height", x => x.Dimensions!.HeightM)
            .Map<Product, decimal?>("volume", x => x.Dimensions!.VolumeM3)
            .Map<Product, decimal?>("weight", x => x.Weight!.WeightKg);
    }
}
