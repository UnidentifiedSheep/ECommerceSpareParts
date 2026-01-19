using Core.Models;

namespace Main.Abstractions.Dtos.Amw.ArticleCharacteristics;

public class PatchCharacteristicsDto
{
    public PatchField<string?> Name { get; set; } = PatchField<string?>.NotSet();
    public PatchField<string> Value { get; set; } = PatchField<string>.NotSet();
}