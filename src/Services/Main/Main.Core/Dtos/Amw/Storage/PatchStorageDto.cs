using Core.Models;
using Main.Core.Enums;

namespace Main.Core.Dtos.Amw.Storage;

public class PatchStorageDto
{
    public PatchField<string?> Description { get; set; } = PatchField<string?>.NotSet();

    public PatchField<string?> Location { get; set; } = PatchField<string?>.NotSet();
    public PatchField<StorageType> Type { get; set; } = PatchField<StorageType>.NotSet();
}