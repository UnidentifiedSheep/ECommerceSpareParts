using Core.Models;
using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.Storage;

public class PatchStorageDto
{
    public PatchField<string?> Description { get; set; } = PatchField<string?>.NotSet();

    public PatchField<string?> Location { get; set; } = PatchField<string?>.NotSet();
    public PatchField<StorageType> Type { get; set; } = PatchField<StorageType>.NotSet();
}