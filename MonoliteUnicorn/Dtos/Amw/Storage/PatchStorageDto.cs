using Core.Json;

namespace MonoliteUnicorn.Dtos.Amw.Storage;

public class PatchStorageDto
{
    public PatchField<string?> Description { get; set; } = PatchField<string?>.NotSet();

    public PatchField<string?> Location { get; set; } = PatchField<string?>.NotSet();
}