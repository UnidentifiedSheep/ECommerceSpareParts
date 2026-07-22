using Abstractions;
using Application.Common.Extensions;
using Main.Entities.Organization;
using Main.Entities.Producer;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.Sale;
using Main.Entities.Storage;
using Main.Enums;
using Main.Enums.Organization;

namespace Main.Application.Configs;

public static class SortByConfig
{
    public static void Configure()
    {
        QueryableSortBy.Value
            .MapDefault<Product, int>(x => x.Id)
            .Map<Product, int>("id", x => x.Id)
            .Map<Product, string>("sku", x => x.Sku.NormalizedValue)
            .Map<Product, string>("name", x => x.Name.Value)
            .Map<Product, int>("count", x => x.Stock.Value)
            .Map<Product, string>("producerName", x => x.Producer.Name)
            .Map<Product, string>("indicator", x => x.Indicator!.Value!)
            .Map<Product, long>("popularity", x => x.Popularity);

        QueryableSortBy.Value
            .MapDefault<Producer, int>(x => x.Id)
            .Map<Producer, int>("id", x => x.Id)
            .Map<Producer, string>("name", x => x.Name);

        QueryableSortBy.Value
            .MapDefault<Organization, string>(x => x.Name)
            .Map<Organization, Guid>("id", x => x.Id)
            .Map<Organization, string>("name", x => x.Name)
            .Map<Organization, string>("systemName", x => x.SystemName)
            .Map<Organization, OrganizationType>("type", x => x.Type);

        QueryableSortBy.Value
            .MapDefault<Purchase, DateTime>(x => x.PurchaseDatetime, true)
            .Map<Purchase, DateTime>("dateTime", x => x.PurchaseDatetime)
            .Map<Purchase, decimal>("totalSum", x => x.Transaction.Amount)
            .Map<Purchase, Guid>("id", x => x.Id);

        QueryableSortBy.Value
            .MapDefault<Sale, DateTime>(x => x.SaleDatetime, true)
            .Map<Sale, DateTime>("dateTime", x => x.SaleDatetime)
            .Map<Sale, decimal>("totalSum", x => x.Transaction.Amount)
            .Map<Sale, Guid>("id", x => x.Id);

        QueryableSortBy.Value
            .MapDefault<ProductReservation, DateTime>(x => x.CreatedAt)
            .Map<ProductReservation, int>("id", x => x.Id)
            .Map<ProductReservation, DateTime>("createAt", x => x.CreatedAt)
            .Map<ProductReservation, DateTime>("updatedAt", x => x.UpdatedAt)
            .Map<ProductReservation, ProductReservationStatus>("status", x => x.Status);

        QueryableSortBy.Value.ConfigureForJob();
    }
}
