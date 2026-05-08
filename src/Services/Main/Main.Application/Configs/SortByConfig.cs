using Abstractions;
using Main.Entities.Producer;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.Sale;
using Main.Entities.Storage;

namespace Main.Application.Configs;

public static class SortByConfig
{
    public static void Configure()
    {
        QueryableSortByOptions.Value
            .MapDefault<Product, int>(x => x.Id)
            .Map<Product, int>("id", x => x.Id)
            .Map<Product, string>("sku", x => x.Sku.NormalizedValue)
            .Map<Product, string>("name", x => x.Name.Value)
            .Map<Product, int>("count", x => x.Stock.Value)
            .Map<Product, string>("producerName", x => x.Producer.Name.Value)
            .Map<Product, string>("indicator", x => x.Indicator!.Value!)
            .Map<Product, long>("popularity", x => x.Popularity);

        QueryableSortByOptions.Value
            .MapDefault<Producer, int>(x => x.Id)
            .Map<Producer, int>("id", x => x.Id)
            .Map<Producer, string>("name", x => x.Name.Value);

        QueryableSortByOptions.Value
            .MapDefault<Purchase, DateTime>(x => x.PurchaseDatetime)
            .Map<Purchase, DateTime>("dateTime", x => x.PurchaseDatetime)
            .Map<Purchase, decimal>("totalSum", x => x.Transaction.Amount)
            .Map<Purchase, Guid>("id", x => x.Id);

        QueryableSortByOptions.Value
            .MapDefault<Sale, DateTime>(x => x.SaleDatetime)
            .Map<Sale, DateTime>("dateTime", x => x.SaleDatetime)
            .Map<Sale, decimal>("totalSum", x => x.Transaction.Amount)
            .Map<Sale, Guid>("id", x => x.Id);

        QueryableSortByOptions.Value
            .MapDefault<StorageContentReservation, DateTime>(x => x.CreatedAt)
            .Map<StorageContentReservation, int>("id", x => x.Id)
            .Map<StorageContentReservation, DateTime>("createAt", x => x.CreatedAt)
            .Map<StorageContentReservation, DateTime>("updatedAt", x => x.UpdatedAt)
            .Map<StorageContentReservation, bool>("isDone", x => x.IsDone);
    }
}