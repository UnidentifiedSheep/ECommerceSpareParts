namespace Main.Application.Models.Storage;

public record StorageLot(
    int Id,
    int ProductId,
    int CurrencyId,
    decimal BuyPrice,
    int Count,
    DateTime PurchaseDatetime);
