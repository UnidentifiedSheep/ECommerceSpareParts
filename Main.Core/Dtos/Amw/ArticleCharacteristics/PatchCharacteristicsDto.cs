using Core.Models;

namespace Main.Core.Dtos.Amw.ArticleCharacteristics;

public class PatchCharacteristicsDto
{
    public PatchField<int> ArticleId = PatchField<int>.NotSet();
    public PatchField<string?> Name = PatchField<string?>.NotSet();
    public PatchField<string> Value = PatchField<string>.NotSet();
}