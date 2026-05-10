namespace Main.Application.Models.Sale;

public record StorageLot(
    int Id,
    int ProductId,
    int CurrencyId,
    decimal BuyPrice,
    int Count,
    DateTime PurchaseDatetime);