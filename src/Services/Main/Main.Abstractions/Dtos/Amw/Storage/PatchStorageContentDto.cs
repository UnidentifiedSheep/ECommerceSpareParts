using Core.Models;

namespace Main.Abstractions.Dtos.Amw.Storage;

public class PatchStorageContentDto
{
    public PatchField<int> Count { get; set; } = PatchField<int>.NotSet();
    public PatchField<DateTime> PurchaseDatetime { get; set; } = PatchField<DateTime>.NotSet();
    public PatchField<decimal> BuyPrice { get; set; } = PatchField<decimal>.NotSet();
    public PatchField<int> CurrencyId { get; set; } = PatchField<int>.NotSet();
}