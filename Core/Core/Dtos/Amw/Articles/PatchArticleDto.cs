using Core.Models;

namespace Core.Dtos.Amw.Articles;

public class PatchArticleDto
{
    public PatchField<string> ArticleNumber { get; set; } = PatchField<string>.NotSet();
    public PatchField<string> ArticleName { get; set; } = PatchField<string>.NotSet();
    public PatchField<int> ProducerId { get; set; } = PatchField<int>.NotSet();
    public PatchField<string?> Description { get; set; } = PatchField<string?>.NotSet();
    public PatchField<bool> IsOe { get; set; } = PatchField<bool>.NotSet();
    public PatchField<int?> PackingUnit { get; set; } = PatchField<int?>.NotSet();
    public PatchField<string?> Indicator { get; set; } = PatchField<string?>.NotSet();
    public PatchField<int?> CategoryId { get; set; } = PatchField<int?>.NotSet();
}