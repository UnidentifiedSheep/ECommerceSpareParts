using Core.Models;

namespace Core.Dtos.Amw.Storage;

public class PatchStorageContentDto
{
    public PatchField<string> StorageName { get; set; } = PatchField<string>.NotSet();
    public PatchField<int> Count { get; set; } = PatchField<int>.NotSet();
    public PatchField<DateTime> PurchaseDatetime { get; set; } = PatchField<DateTime>.NotSet();
    public PatchField<decimal> BuyPrice { get; set; } = PatchField<decimal>.NotSet();
    public PatchField<int> CurrencyId { get; set; } = PatchField<int>.NotSet();
}