using Core.Json;

namespace MonoliteUnicorn.Dtos.Amw.Producers;

public class PatchProducerDto
{
    public PatchField<string> Name { get; set; } = PatchField<string>.NotSet(); 
    public PatchField<bool> IsOe { get; set; } = PatchField<bool>.NotSet();
    public PatchField<string?> Description { get; set; } = PatchField<string?>.NotSet();
}